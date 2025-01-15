using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AssetListManager : MonoBehaviour
{

    public GameObject camera;       // Camera for spawning object


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

        Debug.Log($"Loading assets from: {objectsFolderPath}");
        LoadAssetsFromFolder(objectsFolderPath, false);

        Debug.Log($"Loading assets from: {permanentFolderPath}");
        LoadAssetsFromFolder(permanentFolderPath, true);

        foreach (var asset in assets)
        {
            Debug.Log($"Asset Loaded: {asset.name} | Path: {asset.path} | Permanent: {asset.isPermanent}");
        }
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
            string pngFilePath = Path.Combine(Path.GetDirectoryName(filePath), assetName + ".png");
            if (File.Exists(pngFilePath))
            {
                assets.Add(new Asset(assetName, filePath, pngFilePath, isPermanent)); // Charger la texture PNG si elle existe
            }
            else
            {
                assets.Add(new Asset(assetName, filePath, null, isPermanent));
            }
            
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
        Utils.instantiate3DObj(asset.path, asset.texturePath, camera);
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

    public List<Asset> GetAssets()
    {
        return assets;
    }

}

// Asset class to represent assets in the list
[System.Serializable]
public class Asset
{
    public string name; // Asset name
    public string path; // Path to the asset file
    public string texturePath;
    public bool isPermanent; // Indicates if the asset is permanent

    public Asset(string name, string path, string texturePath, bool isPermanent)
    {
        this.name = name;
        this.path = path;
        this.texturePath = texturePath;
        this.isPermanent = isPermanent;
    }
}
