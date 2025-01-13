using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AssetListManager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject assetButtonPrefab;  // Assign the Button Prefab
    public Transform contentPanel;       // Assign the Scroll View Content

    [Header("Asset Management")]
    public List<Asset> assets = new List<Asset>(); // Store all assets

    void Start()
    {
        LoadInitialAssets();
        PopulateAssetList();
    }

    // Load predefined assets (e.g., from Resources folder or initial setup)
    void LoadInitialAssets()
    {
        AddAsset("Sphere", Resources.Load<GameObject>("Models/Sphere"));
        AddAsset("Cube", Resources.Load<GameObject>("Models/Cube"));
    }

    // Add a new asset to the list and UI dynamically
    public void AddAsset(string name, GameObject model)
    {
        if (model == null)
        {
            Debug.LogError($"Failed to add asset: Model is null for {name}");
            return;
        }

        // Add to the list
        assets.Add(new Asset(name, model));

        // Create a UI button for the asset
        GameObject button = Instantiate(assetButtonPrefab, contentPanel);
        button.transform.localScale = Vector3.one;

        // Set button text
        Text buttonText = button.GetComponentInChildren<Text>();
        buttonText.text = name;

        // Add a click event to spawn the asset
        button.GetComponent<Button>().onClick.AddListener(() => SpawnAsset(name));
    }

    // Populate the UI with buttons for all assets
    void PopulateAssetList()
    {
        foreach (Asset asset in assets)
        {
            AddAsset(asset.name, asset.model);
        }
    }

    // Spawn the selected asset in the scene
    public void SpawnAsset(string assetName)
    {
        Asset asset = assets.Find(a => a.name == assetName);
        if (asset == null)
        {
            Debug.LogError($"Asset not found: {assetName}");
            return;
        }

        // Spawn the asset at a fixed position
        Vector3 spawnPosition = new Vector3(-94f, -91f, 300f);
        GameObject instantiatedAsset = Instantiate(asset.model, spawnPosition, Quaternion.identity);

        // Ensure it has a collider and no gravity
        if (instantiatedAsset.GetComponent<Collider>() == null)
        {
            instantiatedAsset.AddComponent<BoxCollider>();
        }

        Rigidbody rb = instantiatedAsset.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = instantiatedAsset.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
    }
}

// Asset class for managing asset data
[System.Serializable]
public class Asset
{
    public string name;
    public GameObject model;

    public Asset(string name, GameObject model)
    {
        this.name = name;
        this.model = model;
    }
}
