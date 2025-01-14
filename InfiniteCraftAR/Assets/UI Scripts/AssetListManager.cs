using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AssetListManager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject assetButtonPrefab;  // Prefab for asset buttons
    public Transform contentPanel;       // Parent container for buttons

    [Header("File Paths")]
    public string objectsFolderPath = "Assets/Objects";      // Path to Objects folder
    public string permanentFolderPath = "Assets/Objects/Perm"; // Path to permanent subfolder

    private List<Asset> assets = new List<Asset>(); // List to store loaded assets

    void Start()
    {
        LoadAssets();
        PopulateAssetList();
    }

    // Load all assets from the Objects and Perm folders
    void LoadAssets()
    {
        assets.Clear();

        // Load non-permanent assets
        LoadAssetsFromFolder(objectsFolderPath, false);

        // Load permanent assets
        LoadAssetsFromFolder(permanentFolderPath, true);
    }

    // Helper function to load assets from a specified folder
    void LoadAssetsFromFolder(string folderPath, bool isPermanent)
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError($"Folder not found: {folderPath}");
            return;
        }

        string[] files = Directory.GetFiles(folderPath, "*.obj", SearchOption.AllDirectories);
        foreach (string filePath in files)
        {
            string assetName = Path.GetFileNameWithoutExtension(filePath);
            assets.Add(new Asset(assetName, filePath, isPermanent));
        }
    }

    // Populate the Scroll View with buttons
    void PopulateAssetList()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject); // Clear existing buttons
        }

        foreach (Asset asset in assets)
        {
            GameObject button = Instantiate(assetButtonPrefab, contentPanel);
            button.transform.localScale = Vector3.one;

            // Set button text to the asset name (using TextMeshPro)
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = asset.name;
            }
            else
            {
                Debug.LogError("Button prefab is missing a TextMeshProUGUI component.");
            }

            // Add an OnClick listener to spawn the asset
            button.GetComponent<Button>().onClick.AddListener(() => SpawnAsset(asset));
        }
    }

    // Spawn the selected asset into the scene
    void SpawnAsset(Asset asset)
    {
        Debug.Log($"Spawning asset: {asset.name}");

        GameObject loadedObject = LoadObjFromFile(asset.path);
        if (loadedObject != null)
        {
            Vector3 spawnPosition = new Vector3(0, 0, 0); // Adjust position as needed
            Instantiate(loadedObject, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError($"Failed to load asset: {asset.name}");
        }
    }

    // Load an OBJ file and return a GameObject
    private GameObject LoadObjFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            Debug.Log($"Loading OBJ file: {filePath}");
            var objLoader = new Dummiesman.OBJLoader();
            return objLoader.Load(filePath);
        }
        else
        {
            Debug.LogError($"File not found: {filePath}");
            return null;
        }
    }

    // Clear all non-permanent objects from the Objects folder
    public void ClearNonPermanentObjects()
    {
        Debug.Log("Clearing non-permanent objects...");

        if (!Directory.Exists(objectsFolderPath))
        {
            Debug.LogError($"Objects folder not found at: {objectsFolderPath}");
            return;
        }

        string[] files = Directory.GetFiles(objectsFolderPath, "*.*", SearchOption.AllDirectories);

        foreach (string filePath in files)
        {
            if (!IsPermanent(filePath))
            {
                Debug.Log($"Deleting: {filePath}");
                File.Delete(filePath);
            }
        }

        // Reload the asset list after clearing
        LoadAssets();
        PopulateAssetList();
    }

    // Determine if a file is in the permanent folder
    private bool IsPermanent(string filePath)
    {
        string fullPermanentPath = Path.GetFullPath(permanentFolderPath);
        string fullFilePath = Path.GetFullPath(filePath);
        return fullFilePath.StartsWith(fullPermanentPath);
    }
}

// Asset class to represent assets in the list
[System.Serializable]
public class Asset
{
    public string name; // Asset name
    public string path; // Path to the asset file
    public bool isPermanent; // Indicates if the asset is permanent

    public Asset(string name, string path, bool isPermanent)
    {
        this.name = name;
        this.path = path;
        this.isPermanent = isPermanent;
    }
}
