/**
 * @file ImageProcessARVRResized.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Manage video capture resize it and merge with 3D elements from Unity scene.
 */
using System;
using UnityEngine;
using UnityEngine.Rendering;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public class ImageProcessARVRResized : ImageProcessBase
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
        private byte[] rgbBytes; // rgbBytes buffer converted from YUV


        private const UInt32 NEW_WIDTH = 42; // Resized Width
        private const UInt32 NEW_HEIGHT = 42; // Resized Height

        private byte[] resizedRGBABuffer; // RGBA bytes to display once resized

        #region UNITY API
        private void LateUpdate()
        {

            if (SystemInfo.supportsAsyncGPUReadback)
            {
                RenderTexture rt = RenderTexture.GetTemporary((int)NEW_WIDTH, (int)NEW_HEIGHT, 32);
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
                if (m_texture == null)
                {
                    m_action = (rgbaVR) =>
                    {

                        m_texture = new Texture2D((int)NEW_WIDTH, (int)NEW_HEIGHT, TextureFormat.RGBA32, false, false);
                        m_material.mainTexture = m_texture;
                        rgbBytes = new byte[frameInfo.height * frameInfo.width * 4];
                        resizedRGBABuffer = new byte[NEW_WIDTH * NEW_HEIGHT * 4];
                    };
                }
                else
                {
                    // Convert YUV buffer to RGBA buffer
                    LynxOpenCV.YUV2RGBA(frameInfo.leftEyeBuffer, frameInfo.width, frameInfo.height, rgbBytes);

                    // Resize the buffer 
                    LynxOpenCV.ResizeFrame(frameInfo.width, frameInfo.height, 4, NEW_WIDTH, NEW_HEIGHT, rgbBytes, resizedRGBABuffer);

                    // User action to call the UI part in main thread
                    m_action = (rgbaVR) =>
                    {
                        // Blend AR + VR in the frame
                        LynxOpenCV.Compose_ARVR_2_RGBA_from_RGBA_AR(resizedRGBABuffer, NEW_WIDTH, NEW_HEIGHT, rgbaVR);

                        // Display texture
                        LynxOpenCV.SetClock();
                        DisplayBuffer(ref m_texture, rgbaVR);

                    };
                }
            }
        }
    }
}