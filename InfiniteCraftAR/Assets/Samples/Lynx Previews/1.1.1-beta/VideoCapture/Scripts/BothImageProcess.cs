/**
 * @file BothImageProcess.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Manage video capture from RGB cameras and display each eyes in a different texture.
 */

using System;
using UnityEngine;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public class BothImageProcess : ImageProcessBase
    {
        #region INSPECTOR
        [Tooltip("Material used to display the capture")]
        [SerializeField] Material m_materialLeft = null;
        [SerializeField] Material m_materialRight = null;

        [Tooltip("FPS to target for frame capture")]
        [Range(1, 90)]
        [SerializeField] int fps = 90;
        #endregion

        private static Action m_action = null; // Action for UI thread
        private Texture2D m_textureLeft = null; // Texture to handle camera texture
        private Texture2D m_textureRight = null; // Texture to handle camera texture
        private byte[] rgbBytesLeft; // Bytes from converted capture
        private byte[] rgbBytesRight; // Bytes from converted capture

        #region UNITY API
        private void LateUpdate()
        {
            // Fire event from callback in main thread
            if (m_action != null)
            {
                m_action.Invoke();
                m_action = null;
            }
        }
        #endregion

        #region OVERRIDES
        public override void StartVideoCapture()
        {
            LynxCaptureAPI.onRGBFrames += OnCallback;

            // Start video capture at given FPS
            if (!LynxCaptureAPI.StartCapture(LynxCaptureAPI.ESensorType.RGB, fps))
                Debug.LogError("Failed to start camera");
        }
        #endregion

        /// <summary>
        /// Create texturexs and buffer at first frame.
        /// Then, display buffers on textures attached to given materials.
        /// </summary>
        /// <param name="frameInfo">Struct holding info and buffers for each eye</param>
        public void OnCallback(LynxFrameInfo frameInfo)
        {
            if (m_action == null)
            {

                if (m_textureLeft == null) // Create texture
                {
                    m_action = () =>
                    {
                        rgbBytesLeft = new byte[frameInfo.width * frameInfo.height * 3];
                        rgbBytesRight = new byte[frameInfo.width * frameInfo.height * 3];
                        m_textureLeft = new Texture2D((int)frameInfo.width, (int)frameInfo.height, TextureFormat.RGB24, false, false); // AHB
                        m_textureRight = new Texture2D((int)frameInfo.width, (int)frameInfo.height, TextureFormat.RGB24, false, false); // AHB
                        m_materialLeft.mainTexture = m_textureLeft;
                        m_materialRight.mainTexture = m_textureRight;
                    };
                }
                else
                {
                    // Convert YUV buffers to RGB buffers
                    LynxOpenCV.YUV2RGB(frameInfo.leftEyeBuffer, frameInfo.width, frameInfo.height, rgbBytesLeft);
                    LynxOpenCV.YUV2RGB(frameInfo.rightEyeBuffer, frameInfo.width, frameInfo.height, rgbBytesRight);

                    // Display buffer in main thread
                    m_action = () => DisplayBuffer(rgbBytesLeft, rgbBytesRight, ref m_textureLeft, ref m_textureRight); // Display RGB Frames
                }
            }
        }

        /// <summary>
        /// Apply array bytes to the texture
        /// </summary>
        /// <param name="data">Image buffer</param>
        /// <param name="size">Size of the buffer</param>
        public void DisplayBuffer(byte[] managedArrayLeft, byte[] managedArrayRight, ref Texture2D texLeft, ref Texture2D texRight)
        {

            try
            {
                texLeft.LoadRawTextureData(managedArrayLeft);
                texRight.LoadRawTextureData(managedArrayRight);

                texLeft.Apply();
                texRight.Apply();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }

            GC.Collect();
        }
    }
}