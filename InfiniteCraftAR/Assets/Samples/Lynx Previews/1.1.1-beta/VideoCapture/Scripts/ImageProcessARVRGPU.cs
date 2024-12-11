/**
 * @file ImageProcessARVRGPU.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Manage video capture and display it in a given material. Process for YUV to RGB conversion + compositing with Unity scene are done by the shader.
 */

using System;
using UnityEngine;
using UnityEngine.Rendering;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public class ImageProcessARVRGPU : ImageProcessBase
    {
        #region INSPECTOR
        [Tooltip("Material used to display the capture")]
        [SerializeField] Material m_material = null;

        [Tooltip("FPS to target for frame capture")]
        [Range(1, 90)]
        [SerializeField] int fps = 90;
        #endregion

        private static Action<byte[]> m_action = null; // Action for UI thread
        private Texture2D m_textureAR = null; // Texture to handle camera texture
        private Texture2D m_textureVR = null; // Texture to handle camera texture

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
        /// Create texturexs at first frame.
        /// Then, update textures with frame data.
        /// </summary>
        /// <param name="frameInfo">Struct holding info and buffers for each eye</param>
        private void OnCallbackFrame(LynxFrameInfo frameInfo)
        {
            if (m_action == null)
            {

                if (m_textureAR == null)
                {
                    m_action = (rgbaBytes) =>
                    {
                        m_textureAR = new Texture2D((int)frameInfo.width, (int)(frameInfo.height * 1.5), TextureFormat.R8, false, false);
                        m_textureAR.filterMode = FilterMode.Point;
                        m_material.mainTexture = m_textureAR;

                        m_textureVR = new Texture2D((int)frameInfo.width, (int)frameInfo.height, TextureFormat.RGBA32, false, false);
                        m_material.SetTexture("_TexVR", m_textureVR);
                    };
                }
                else
                {
                    m_action = (rgbaBytes) =>
                    {
                        // Blend AR + VR in same image
                        try
                        {
                            m_textureAR.LoadRawTextureData(frameInfo.leftEyeBuffer, (int)frameInfo.bufferSize);
                            m_textureAR.Apply();

                            m_textureVR.LoadRawTextureData(rgbaBytes);
                            m_textureVR.Apply();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(ex.ToString());
                        }

                        GC.Collect();
                    };
                }
            }
        }
    }
}