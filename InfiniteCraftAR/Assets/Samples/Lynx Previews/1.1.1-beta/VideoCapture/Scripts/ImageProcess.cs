/**
 * @file ImageProcess.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Manage video capture callback and display it in a given material.
 */

using System;
using UnityEngine;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public class ImageProcess : ImageProcessBase
    {
        #region INSPECTOR
        [Tooltip("Material used to display the capture")]
        [SerializeField] Material m_material = null;

        [Tooltip("FPS to target for frame capture")]
        [Range(1, 90)]
        [SerializeField] int fps = 90;

        [Tooltip("True to dedistord fisheye. False to keep raw fisheye distortion.")]
        public bool rectifyFishEye = true;
        #endregion

        private static Action m_action = null; // Action for UI thread
        private Texture2D m_texture = null; // Texture to handle camera texture
        private byte[] m_rgbBuffer = null; // RGB buffer to display

        

        #region UNITY API
        private void LateUpdate()
        {
            if(Input.GetKeyUp(KeyCode.F))
                rectifyFishEye = !rectifyFishEye;

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

            // Read camera parameters and init undistortion maps
            IntrinsicData leftIntrinsics;
            ExtrinsicData leftExtrinsics;
            LynxCaptureAPI.ReadCameraParameters(LynxCaptureAPI.ESensorType.RGB, LynxCaptureAPI.ESensorEye.LEFT, out leftIntrinsics, out leftExtrinsics);
            LynxOpenCV.InitUndistortRectifyMap(0, ref leftIntrinsics, ref leftExtrinsics);



            IntrinsicData rightIntrinsics;
            ExtrinsicData rightExtrinsics;
            LynxCaptureAPI.ReadCameraParameters(LynxCaptureAPI.ESensorType.RGB, LynxCaptureAPI.ESensorEye.RIGHT, out rightIntrinsics, out rightExtrinsics);
            Debug.Log($"Left" +
                $"\n\t{leftExtrinsics.position[0]}\t{leftExtrinsics.position[1]}\t{leftExtrinsics.position[2]}" +
                $"\n\t{leftExtrinsics.orientation[0]}\t{leftExtrinsics.orientation[1]}\t{leftExtrinsics.orientation[2]}\t{leftExtrinsics.orientation[3]}" +
                $"\n\t{leftIntrinsics.size[0]}\t{leftIntrinsics.size[1]}" +
                $"\n\t{leftIntrinsics.principalPoint[0]}\t{leftIntrinsics.principalPoint[1]}" +
                $"\n\t{leftIntrinsics.focalLength[0]}\t{leftIntrinsics.focalLength[1]}" +
                $"\n\t{leftIntrinsics.radialDistortion[0]}\t{leftIntrinsics.radialDistortion[1]}\t{leftIntrinsics.radialDistortion[2]}\t{leftIntrinsics.radialDistortion[3]}" +
                $"\n\t{leftIntrinsics.tangentialDistortion[0]}\t{leftIntrinsics.tangentialDistortion[1]}" +
                $"");

            Debug.Log($"Right" +
                $"\n\t{rightExtrinsics.position[0]}\t{rightExtrinsics.position[1]}\t{rightExtrinsics.position[2]}" +
                $"\n\t{rightExtrinsics.orientation[0]}\t{rightExtrinsics.orientation[1]}\t{rightExtrinsics.orientation[2]}\t{rightExtrinsics.orientation[3]}" +
                $"\n\t{rightIntrinsics.size[0]}\t{rightIntrinsics.size[1]}" +
                $"\n\t{rightIntrinsics.principalPoint[0]}\t{rightIntrinsics.principalPoint[1]}" +
                $"\n\t{rightIntrinsics.focalLength[0]}\t{rightIntrinsics.focalLength[1]}" +
                $"\n\t{rightIntrinsics.radialDistortion[0]}\t{rightIntrinsics.radialDistortion[1]}\t{rightIntrinsics.radialDistortion[2]}\t{rightIntrinsics.radialDistortion[3]}" +
                $"\n\t{rightIntrinsics.tangentialDistortion[0]}\t{rightIntrinsics.tangentialDistortion[1]}" +
                $"");

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

                if (m_texture == null) // Create texture and bind material
                {
                    m_action = () =>
                    {
                        m_rgbBuffer = new byte[frameInfo.width * frameInfo.height * 3];
                        m_texture = new Texture2D((int)frameInfo.width, (int)frameInfo.height, TextureFormat.RGB24, false, false); // AHB
                        m_material.mainTexture = m_texture;
                    };
                }
                else // Display RGB Frame
                {
                    if (rectifyFishEye)
                    {
                        // Convert to RGB and apply fisheye undistortion.
                        LynxOpenCV.UndistordRGBFromYUV(0, frameInfo.leftEyeBuffer, frameInfo.width, frameInfo.height, m_rgbBuffer);
                    }
                    else
                    {
                        // Uncomment to display without fisheye undistortion
                        LynxOpenCV.YUV2RGB(frameInfo.leftEyeBuffer, frameInfo.width, frameInfo.height, m_rgbBuffer); 
                    }

                    m_action = () => DisplayBuffer(ref m_texture, m_rgbBuffer);
                }
            }
        }
    }
}