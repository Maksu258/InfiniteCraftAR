using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Lynx;

public class Screenshot : MonoBehaviour
{
    public APIManager api;

    public GameObject mainCamera;

    public ScreenshotAndVideoUtilities screenUtils;

    public void Trigger()
    {
        screenUtils = new ScreenshotAndVideoUtilities();
        screenUtils.m_cameraGameObjectForScreenShot = mainCamera;
        // api = new APIManager();
        Debug.Log("Start Screenshot");
        screenUtils.TakeScreenShot(1024,1024);
        Debug.Log("End Screenshot");

        string folderPath = Path.Combine(Application.dataPath, "ScreenAndVideoShots");
        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning("No screenshot in " + folderPath);
            return;
        }
        string latestFilePath = GetLatestFilePath(folderPath);

        api.analyzeImage(latestFilePath);
    }

    public string GetLatestFilePath(string folderPath)
    {
        var files = Directory.GetFiles(folderPath);

        var latestFile = files
            .Select(file => new FileInfo(file))
            .OrderByDescending(fileInfo => fileInfo.LastWriteTime)
            .FirstOrDefault();

        return latestFile?.FullName;
    }
}
