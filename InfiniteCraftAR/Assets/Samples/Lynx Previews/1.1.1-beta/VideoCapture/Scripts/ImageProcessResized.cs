/**
 * @file ImageProcessResized.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Manage video capture from RGB camera, resize the buffer and display it.
 */

using System;
using UnityEngine;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public class ImageProcessResized : ImageProcessBase
    {
        #region INSPECTOR
        [Tooltip("Material used to display the capture")]
        [SerializeField] Material m_material = null;

        [Tooltip("FPS to target for frame capture")]
        [Range(1, 90)]
        [SerializeField] int fps = 90;
        #endregion

        private static Action m_action = null; // Action for UI thread
        private Texture2D m_texture = null; // Texture to handle camera texture
        private byte[] rgbBytes; // Bytes from converted capture

        private const uint NEW_WIDTH = 80; // Resized Width
        private const uint NEW_HEIGHT = 64; // Resized Height

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
            LynxCaptureAPI.onRGBFrames += OnCallbackFrame;

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
        public void OnCallbackFrame(LynxFrameInfo frameInfo)
        {
            if (m_action == null)
            {
                if (m_texture == null)  // Create texture
                {

                    m_action = () =>
                    {
                        rgbBytes = new byte[frameInfo.width * frameInfo.height * 3];
                        m_texture = new Texture2D((int)NEW_WIDTH, (int)NEW_HEIGHT, TextureFormat.RGB24, false, false); // AHB
                        m_material.mainTexture = m_texture;
                    };
                }
                else  // Display resized RGB Frame
                {
                    // Convert YUV pointer to RGB buffer
                    LynxOpenCV.YUV2RGB(frameInfo.leftEyeBuffer, frameInfo.width, frameInfo.height, rgbBytes);

                    // Resize the frame
                    byte[] rgbBytesResized = new byte[NEW_WIDTH * NEW_HEIGHT * 3];
                    LynxOpenCV.ResizeFrame(frameInfo.width, frameInfo.height, 3u, NEW_WIDTH, NEW_HEIGHT, rgbBytes, rgbBytesResized);

                    // Display
                    m_action = () => DisplayBuffer(ref m_texture, rgbBytesResized);
                }
            }
        }
    }
}