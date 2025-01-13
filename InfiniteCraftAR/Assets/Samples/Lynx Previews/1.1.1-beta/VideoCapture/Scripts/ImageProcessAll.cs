/**
 * @file ImageProcessAll.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Display images from all sensors.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using static Lynx.LynxCaptureLibraryInterface;

namespace Lynx
{
    public class ImageProcessAll : ImageProcessBase
    {
        // Materials for each french streaming
        public Material RGBLeftMaterial = null;
        public Material RGBRightMaterial = null;
        public Material TrackingLeftMaterial = null;
        public Material TrackingRightMaterial = null;
        public Material HandtrackingLeftMaterial = null;
        public Material HandtrackingRightMaterial = null;

        [Tooltip("Use RGB camera streaming")]
        public bool m_enableRGB = true;
        [Tooltip("Use 6dof tracking camera streaming")]
        public bool m_enableTracking = true;
        [Tooltip("Use handtracking camera streaming")]
        public bool m_enableHandtracking = true;

        protected Action m_RGBAction = null;
        protected Action m_TrackingAction = null;
        protected Action m_HandtrackingAction = null;

        private const int FPS = 30;

        /*
         * Handle cameras textures
         * [0] RGB Left              [1] RGB Right
         * [2] Tracking Left         [3] Tracking Right
         * [4] Handtracking Left     [5] Handtracking Right
         * */
        private Texture2D[] m_textures = new Texture2D[6];

        // Handle texture dimension by sensor
        private static readonly Dictionary<LynxCaptureAPI.ESensorType, (int width, int height)> TEXTURES_DIMS = new Dictionary<LynxCaptureAPI.ESensorType, (int width, int height)>
        {
            { LynxCaptureAPI.ESensorType.RGB, (1536, 1404) },
            { LynxCaptureAPI.ESensorType.TRACKING, (1280, 400) },
            { LynxCaptureAPI.ESensorType.HANDTRACKING, (400, 400) }
        };

        #region UNITY API
        private void LateUpdate()
        {
            CallAction(ref m_RGBAction);
            CallAction(ref m_TrackingAction);
            CallAction(ref m_HandtrackingAction);
        }
        #endregion

        #region OVERRIDES
        public override void StartVideoCapture()
        {
            ////////// RGB //////////
            if (m_enableRGB)
            {
                // Create textures
                InitTexture(ref m_textures[0], ref RGBLeftMaterial, LynxCaptureAPI.ESensorType.RGB, TextureFormat.RGB24);
                InitTexture(ref m_textures[1], ref RGBRightMaterial, LynxCaptureAPI.ESensorType.RGB, TextureFormat.RGB24);
                int size = (int)(TEXTURES_DIMS[LynxCaptureAPI.ESensorType.RGB].width * TEXTURES_DIMS[LynxCaptureAPI.ESensorType.RGB].height * 3u);
                byte[] rgbLeftFrame = new byte[size];
                byte[] rgbRightFrame = new byte[size];

                // Register callbacks
                LynxCaptureAPI.onRGBFrames += (LynxFrameInfo frameInfo) =>
                {

                    LynxOpenCV.YUV2RGB(frameInfo.leftEyeBuffer, frameInfo.width, frameInfo.height, rgbLeftFrame);
                    LynxOpenCV.YUV2RGB(frameInfo.rightEyeBuffer, frameInfo.width, frameInfo.height, rgbRightFrame);

                    if (m_RGBAction == null)
                    {
                        m_RGBAction = () =>
                        {
                            m_textures[0].LoadRawTextureData(rgbLeftFrame);
                            m_textures[0].Apply();

                            m_textures[1].LoadRawTextureData(rgbRightFrame);
                            m_textures[1].Apply();
                        };
                    }
                };

                // Start capture
                LynxCaptureAPI.StartCapture(LynxCaptureAPI.ESensorType.RGB, FPS);
            }

            ////////// TRACKING //////////
            if (m_enableTracking)
            {
                InitTexture(ref m_textures[2], ref TrackingLeftMaterial, LynxCaptureAPI.ESensorType.TRACKING, TextureFormat.R8);
                InitTexture(ref m_textures[3], ref TrackingRightMaterial, LynxCaptureAPI.ESensorType.TRACKING, TextureFormat.R8);

                // Register callbacks
                LynxCaptureAPI.onTrackingFrames += (LynxFrameInfo frameInfo) =>
                {
                    if (m_TrackingAction == null)
                    {
                        m_TrackingAction = () =>
                        {
                            m_textures[2].LoadRawTextureData(frameInfo.leftEyeBuffer, (int)frameInfo.bufferSize);
                            m_textures[2].Apply();

                            m_textures[3].LoadRawTextureData(frameInfo.rightEyeBuffer, (int)frameInfo.bufferSize);
                            m_textures[3].Apply();
                        };
                    }
                };


                // Start capture
                LynxCaptureAPI.StartCapture(LynxCaptureAPI.ESensorType.TRACKING, FPS);
            }

            ////////// HANDTRACKING //////////
            if (m_enableHandtracking)
            {
                InitTexture(ref m_textures[4], ref HandtrackingLeftMaterial, LynxCaptureAPI.ESensorType.HANDTRACKING, TextureFormat.R8);
                InitTexture(ref m_textures[5], ref HandtrackingRightMaterial, LynxCaptureAPI.ESensorType.HANDTRACKING, TextureFormat.R8);

                // Register callbacks
                LynxCaptureAPI.onHandtrackingFrames += (LynxFrameInfo frameInfo) =>
                {
                    if (m_HandtrackingAction == null)
                    {
                        m_HandtrackingAction = () =>
                        {
                            m_textures[4].LoadRawTextureData(frameInfo.leftEyeBuffer, (int)frameInfo.bufferSize);
                            m_textures[4].Apply();

                            m_textures[5].LoadRawTextureData(frameInfo.rightEyeBuffer, (int)frameInfo.bufferSize);
                            m_textures[5].Apply();
                        };
                    }
                };


                // Start capture
                LynxCaptureAPI.StartCapture(LynxCaptureAPI.ESensorType.HANDTRACKING, FPS);
            }
        }
        #endregion


        /// <summary>
        /// Initialize texture for a given sensor et texture format
        /// </summary>
        /// <param name="texture">Texture holding the sensor buffer.</param>
        /// <param name="material">Material on which to apply texture</param>
        /// <param name="sensor">Sensor type to use</param>
        /// <param name="textureFormat">Define texture format (number of byte per pixel in the buffer)</param>
        private void InitTexture(ref Texture2D texture, ref Material material, LynxCaptureAPI.ESensorType sensor, TextureFormat textureFormat)
        {
            texture = new Texture2D(TEXTURES_DIMS[sensor].width, TEXTURES_DIMS[sensor].height, textureFormat, false, false); // Create texture
            material.mainTexture = texture; // Attach textures to material
        }

        /// <summary>
        /// Create texturexs and buffer at first frame.
        /// Then, display buffers on textures attached to given materials.
        /// </summary>
        /// <param name="frameInfo">Struct holding info and buffers for each eye</param>
        void CallAction(ref Action action)
        {
            if (action != null)
            {
                action.Invoke();
                action = null;
            }
        }
    }
}