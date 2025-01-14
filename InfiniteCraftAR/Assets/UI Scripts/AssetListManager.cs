using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class AssetListManager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject assetButtonPrefab;  // Prefab for the buttons
    public Transform contentPanel;       // Parent container for buttons

    [Header("File System")]
    public string objectsFolderPath = "Assets/Objects"; // Path to the Objects folder

    private List<Asset> assets = new List<Asset>(); // List to store all assets

    void Start()
    {
        LoadAssetsFromObjectsFolder();
        PopulateAssetList();
    }

    // Load assets dynamically from the Objects folder
    void LoadAssetsFromObjectsFolder()
    {
        if (!Directory.Exists(objectsFolderPath))
        {
            Debug.LogError($"Objects folder not found at: {objectsFolderPath}");
            return;
        }

        string[] objFiles = Directory.GetFiles(objectsFolderPath, "*.obj");

        foreach (string objFilePath in objFiles)
        {
            string assetName = Path.GetFileNameWithoutExtension(objFilePath);
            string relativePath = objFilePath.Replace("\\", "/");

            // Create a placeholder GameObject for now (can use a real OBJ loader later)
            GameObject placeholderModel = new GameObject(assetName);

            // Add the asset to the list
            assets.Add(new Asset(assetName, relativePath, placeholderModel));
        }
    }

    // Populate the Scroll View with buttons
    void PopulateAssetList()
    {
        foreach (Asset asset in assets)
        {
            GameObject button = Instantiate(assetButtonPrefab, contentPanel);
            button.transform.localScale = Vector3.one;

            // Set the button's text
            Text buttonText = button.GetComponentInChildren<Text>();
            buttonText.text = asset.name;

            // Add a click event to spawn the asset
            button.GetComponent<Button>().onClick.AddListener(() => SpawnAsset(asset));
        }
    }

    // Spawn the asset into the scene
    public void SpawnAsset(Asset asset)
    {
        Debug.Log($"Spawning asset: {asset.name}");

        // Load the actual 3D object from the file
        GameObject loadedObject = LoadObjFromFile(asset.path);

        if (loadedObject != null)
        {
            Vector3 spawnPosition = new Vector3(0, 0, 0); // Change to your desired spawn position
            Instantiate(loadedObject, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError($"Failed to load asset: {asset.name}");
        }
    }

    // Load the OBJ file and return a GameObject
    private GameObject LoadObjFromFile(string filePath)
    {
        // Use a real OBJ loader (e.g., Dummiesman.OBJLoader) to load the model
        if (File.Exists(filePath))
        {
            Debug.Log($"Loading OBJ file: {filePath}");
            var objLoader = new Dummiesman.OBJLoader();
            GameObject loadedObj = objLoader.Load(filePath);
            return loadedObj;
        }
        else
        {
            Debug.LogError($"OBJ file not found: {filePath}");
            return null;
        }
    }
}

// Asset class to store asset data
[System.Serializable]
public class Asset
{
    public string name;   // Name of the asset
    public string path;   // Path to the OBJ file
    public GameObject model; // Placeholder or actual loaded model

    public Asset(string name, string path, GameObject model)
    {
        this.name = name;
        this.path = path;
        this.model = model;
    }
}
