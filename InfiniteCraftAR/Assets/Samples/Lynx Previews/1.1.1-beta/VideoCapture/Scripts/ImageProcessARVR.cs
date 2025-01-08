/**
 * @file ImageProcess.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Base script to manage video capture callback and display it in a given material.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public class ImageProcessARVR : ImageProcessBase
    {
        #region INSPECTOR
        [Tooltip("Material used to display the capture")]
        [SerializeField] Material m_material = null;

        [Tooltip("FPS to target for frame capture")]
        [Range(1, 90)]
        [SerializeField] int fps = 90;
        #endregion

        private static Action<byte[]> m_action = null; // Action for UI thread
        private Texture2D m_texture = null; // Texture to handle camera texture

        #region UNITY API
        private void LateUpdate()
        {
            if (SystemInfo.supportsAsyncGPUReadback)
            {
                RenderTexture rt = RenderTexture.GetTemporary(1536, 1404, 32);
                Camera.main.targetTexture = rt;
                Camera.main.Render();
                AsyncGPUReadback.Request(rt, 0, TextureFormat.RGBA32, (AsyncGPUReadbackRequest request) =>
                {
                    // Fire event from callback in main thread
                    if (m_action != null)
                    {
                        m_action.Invoke(request.GetData<byte>().ToArray());
                        m_action = null;
                    }
                });

                Camera.main.targetTexture = null;
                RenderTexture.ReleaseTemporary(rt);
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

            if (SystemInfo.supportsAsyncGPUReadback)
                Debug.Log("Support async GPU readback");
            else
                Debug.LogError("Does not support GPU readback");
        }
        #endregion

        /// <summary>
        /// Create texturexs and buffer at first frame.
        /// Then, display buffers on textures attached to given materials.
        /// </summary>
        /// <param name="frameInfo">Struct holding info and buffers for each eye</param>
        private void OnCallbackFrame(LynxFrameInfo frameInfo)
        {
            if (m_action == null)
            {

                if(m_texture == null)
                {
                    m_action = (rgbaBytes) =>
                    {
                        m_texture = new Texture2D((int)frameInfo.width, (int)frameInfo.height, TextureFormat.RGBA32, false, false);
                        m_material.mainTexture = m_texture;
                    };
                }
                else
                {
                    m_action = (rgbaBytes) =>
                    {
                        // Blend AR + VR in same image
                        LynxOpenCV.Compose_ARVR_2_RGBA_From_YUV_AR(frameInfo.leftEyeBuffer, frameInfo.width, frameInfo.height, rgbaBytes);
                        DisplayBuffer(ref m_texture, rgbaBytes);
                    };
                }

                
            }
        }
    }
}