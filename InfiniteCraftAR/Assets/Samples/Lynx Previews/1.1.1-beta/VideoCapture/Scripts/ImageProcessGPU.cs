/**
 * @file ImageProcessGPU.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Base script to manage video capture callback and display it in a given material.
 */

using System;
using UnityEngine;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public class ImageProcessGPU : ImageProcessBase
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
            LynxCaptureAPI.onRGBFrames += OnCallbackFirstFrame;

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
        public void OnCallbackFirstFrame(LynxFrameInfo frameInfo)
        {
            if (m_action == null)
            {
                
                if (m_texture == null) // Create texture and bind material
                {
                    m_action = () =>
                    {
                        rgbBytes = new byte[(int)(frameInfo.width * frameInfo.height * 1.5)];
                        m_texture = new Texture2D((int)frameInfo.width, (int)(frameInfo.height * 1.5), TextureFormat.R8, false, false); // AHB
                        m_texture.filterMode = FilterMode.Point;
                        m_material.mainTexture = m_texture;
                    };
                }
                else // Display RGB Frame
                {
                    //Marshal.Copy(frameInfo.leftEyeBuffer, rgbBytes, 0, (int)(frameInfo.width * frameInfo.height * 1.5));
                    //m_action = () => DisplayBuffer(ref m_texture, rgbBytes);
                    m_action = () =>
                    {
                        try
                        {
                            m_texture.LoadRawTextureData(frameInfo.leftEyeBuffer, (int)(frameInfo.width * frameInfo.height * 1.5));
                            m_texture.Apply();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(ex.ToString());
                        }

                        GC.Collect(); // Frame callback can be so fast than Unity has not the time to use Garbage Collector to remove previous one from memory
                    };
                }
            }
        }
    }
}