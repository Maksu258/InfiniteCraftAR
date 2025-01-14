LynxCaptureAPI.StartCapture(LynxCaptureAPI.ESensorType.RGB, fps); // Open RGB cameras stream
LynxCaptureAPI.StartCapture(LynxCaptureAPI.ESensorType.TRACKING, fps); // Open Tracking cameras stream
LynxCaptureAPI.StartCapture(LynxCaptureAPI.ESensorType.HANDTRACKING, fps); // Open Handtracking cameras stream

LynxCaptureAPI.onRGBFrames += OnCallback; // Attach function called at each frame from the RGB stream
LynxCaptureAPI.onTrackingFrames+= OnCallback; // Attach function called at each frame from the Tracking stream
LynxCaptureAPI.onHandtrackingFrames+= OnCallback; // Attach function called at each frame from the Handtracking stream
﻿
public void OnCallback(LynxFrameInfo frameInfo)
{
    // Process the frame here
}

LynxCaptureAPI.StopCamera(LynxCaptureAPI.ESensorType.RGB); // Stop RGB cameras stream
LynxCaptureAPI.StopCamera(LynxCaptureAPI.ESensorType.TRACKING); // Stop Tracking cameras stream
LynxCaptureAPI.StopCamera(LynxCaptureAPI.ESensorType.HANDTRACKING); // Stop Handtracking cameras stream
LynxCaptureAPI.StopAllCameras(); // Stop all cameras stream

public static bool IsCaptureRunning; // Check if capture is currently running
public static OnFrameDelegate ﻿onRGBFrames; // To subscribe RGB cameras stream
public static OnFrameDelegate ﻿onTrackingFrames; // To subscribe Tracking cameras stream
public static OnFrameDelegate ﻿onHandtrackingFrames; // To subscribe Handtracking cameras stream
public static bool StartCapture(ESensorType sensorType, int maxFPS = 30) // To start cameras for the given type at an expected framerate.
public static void StopCapture(ESensorType sensorType); // To stop cameras for the given type.
public static void StopAllCameras(); // To stop all running cameras
public static bool ReadCameraParameters(ESensorType sensorType, out LynxCaptureLibraryInterface.IntrinsicData intrinsic, out LynxCaptureLibraryInterface.ExtrinsicData extrinsic); // Return intrinsic and extrinsic data for the given camera type.

LynxOpenCV.YUV2RGB(IntPtr YUVBuffer, uint width, uint height, byte[] outRGBBuffer); // Convert YUV buffer to RGB buffer
LynxOpenCV.YUV2RGBA(IntPtr YUVBuffer, uint width, uint height, byte[] outRGBBuffer); // Convert YUV buffer to RGBA buffer
LynxOpenCV.Compose_ARVR_2_RGBA_From_YUV_AR(IntPtr inYUVBufferAR, uint width, uint height, byte[] inOutRGBABufferVR); // Blend AR YUV and VR RGBA together based on VR alpha
LynxOpenCV.Compose_ARVR_2_RGBA_from_RGBA_AR(byte[] inRGBABufferAR, uint width, uint height, byte[] inOutRGBABufferVR); // Blend AR RGBA and VR RGBA together based on VR alpha
LynxOpenCV.ResizeFrame(uint width, uint height, uint depth, uint new_width, uint new_height, byte[] inBuffer, byte[] outBuffer); // Resize buffer from byte array
LynxOpenCV.ResizeFrame(uint width, uint height, uint depth, uint new_width, uint new_height, IntPtr inBuffer, byte[] outBuffer); // Resize buffer from IntPtr
LynxOpenCV.FlipRGBAFrame(uint width, uint height, bool verticalFlip, bool HorizontalFlip, byte[] inOutBuffer); // Flip a RGBA buffer horizontally and/or vertically
LynxOpenCV.FlipRGBFrame(uint width, uint height, bool verticalFlip, bool HorizontalFlip, byte[] inOutBuffer); // Flip a RGBA buffer horizontally and/or vertically
