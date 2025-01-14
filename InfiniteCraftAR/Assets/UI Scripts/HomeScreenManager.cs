using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class HomeScreenManager : MonoBehaviour
{
    [Header("App Settings")]
    public string mainSceneName = "MainScene";         // Name of the main AR scene
    public string objectsFolderPath = "Assets/Objects"; // Path to the Objects folder
    public string permanentFolderPath = "Assets/Objects/Perm"; // Path to the permanent objects folder

    public void StartApp()
    {
        // Load the main AR scene
        SceneManager.LoadScene(mainSceneName);
    }

    public void StartHomeMenu()
    {
        SceneManager.LoadScene("HomeMenu");
    }
    public void ResetProgress()
    {
        Debug.Log("Resetting progress...");

        // Get all files in the Objects folder
        string[] files = Directory.GetFiles(objectsFolderPath);

        foreach (string file in files)
        {
            if (!IsPermanent(file))
            {
                Debug.Log($"Deleting: {file}");
                File.Delete(file);
            }
        }

        Debug.Log("Progress reset complete.");
    }

    private bool IsPermanent(string filePath)
    {
        // Check if the file is in the permanent folder
        string fullPermanentFolderPath = Path.GetFullPath(permanentFolderPath);
        string fullFilePath = Path.GetFullPath(filePath);

        return fullFilePath.StartsWith(fullPermanentFolderPath);
    }

    public void ExitApp()
    {
        Debug.Log("Exiting application...");
        Application.Quit();

        // If running in the Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
