using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AssetListManager : MonoBehaviour
{
    // Assign these in the Inspector
    public GameObject assetButtonPrefab;  // The button prefab
    public Transform contentPanel;       // The Content object of the Scroll View

    // Example asset structure
    private List<Asset> assets = new List<Asset>();

    void Start()
    {
        // Load example assets (replace with your dynamic loading logic)
        LoadAssets();

        // Populate the UI with buttons
        PopulateAssetList();
    }


    void LoadAssets()
    {
        GameObject sphereModel = Resources.Load<GameObject>("Models/Sphere");
        assets.Add(new Asset("Sphere", "https://example.com/car", true, sphereModel));

        GameObject cubeModel = Resources.Load<GameObject>("Models/Cube");
        assets.Add(new Asset(" Cube", "https://example.com/car", true, cubeModel));

    }


    void PopulateAssetList()
    {
        foreach (Asset asset in assets)
        {
            // Instantiate a button from the prefab
            GameObject button = Instantiate(assetButtonPrefab, contentPanel);
            button.transform.localScale = Vector3.one;  // Adjust the scale if necessary

            // Set the button text to the asset's name
            Text buttonText = button.GetComponentInChildren<Text>();
            buttonText.text = asset.name;

            // Debug to check that buttons are being created and listeners added
            Debug.Log($"Adding listener to button for asset: {asset.name}");

            // Add the OnClick listener for the button
            button.GetComponent<Button>().onClick.AddListener(() => OnAssetSelected(asset));
        }
    }



    public void OnAssetSelected(Asset asset)
    {
        Debug.Log("Selected asset: " + asset.name);

        Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;
        GameObject instantiatedAsset = Instantiate(asset.model, spawnPosition, Quaternion.identity);

        // Ensure the instantiated model has a Collider
        if (instantiatedAsset.GetComponent<Collider>() == null)
        {
            instantiatedAsset.AddComponent<BoxCollider>();
        }

        // Add the AssetBehavior script
        instantiatedAsset.AddComponent<AssetBehavior>();
    }


}

// Asset class definition
[System.Serializable]
public class Asset
{
    public string name;      // Name of the asset
    public string link;      // URL or link to an external resource
    public bool permanent;   // Indicates if the asset is permanent
    public GameObject model; // The actual 3D model

    public Asset(string name, string link, bool permanent, GameObject model)
    {
        this.name = name;
        this.link = link;
        this.permanent = permanent;
        this.model = model;
    }
}