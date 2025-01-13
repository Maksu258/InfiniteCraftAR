using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AssetListManager : MonoBehaviour
{
    public GameObject assetButtonPrefab; // Assign the Button Prefab
    public Transform contentPanel;      // Assign the Scroll View Content
    public List<Asset> assets = new List<Asset>(); // List of assets

    void Start()
    {
        // Populate the asset list (add your assets here)
        LoadAssets();

        // Dynamically create buttons for each asset
        PopulateAssetList();
    }

    void LoadAssets()
    {
        // Example assets (replace with your actual resources or models)
        GameObject sphereModel = Resources.Load<GameObject>("Objects/Sphere");
        GameObject cubeModel = Resources.Load<GameObject>("Objets/Cube");

        assets.Add(new Asset("Sphere", sphereModel));
        assets.Add(new Asset("Cube", cubeModel));
    }

    void PopulateAssetList()
    {
        foreach (Asset asset in assets)
        {
            // Instantiate a button for each asset
            GameObject button = Instantiate(assetButtonPrefab, contentPanel);
            button.transform.localScale = Vector3.one;

            // Set the button's text to the asset's name
            Text buttonText = button.GetComponentInChildren<Text>();
            buttonText.text = asset.name;

            // Add a listener to spawn the asset when the button is clicked
            button.GetComponent<Button>().onClick.AddListener(() => SpawnAsset(asset));
        }
    }

    void SpawnAsset(Asset asset)
    {
        // Spawn the asset in front of the user
        Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;
        Instantiate(asset.model, spawnPosition, Quaternion.identity);
    }
}

// Asset class definition
[System.Serializable]
public class Asset
{
    public string name;      // Name of the asset
    public GameObject model; // The 3D model of the asset

    public Asset(string name, GameObject model)
    {
        this.name = name;
        this.model = model;
    }
}
