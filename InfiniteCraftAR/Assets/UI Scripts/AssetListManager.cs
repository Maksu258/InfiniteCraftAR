using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AssetListManager : MonoBehaviour
{
    public GameObject assetButtonPrefab;
    public Transform contentPanel;
    public Camera mainCamera;

    private List<Asset> assets = new List<Asset>();

    void Start()
    {
        LoadAssets();
        PopulateAssetList();
    }

    void LoadAssets()
    {
        GameObject sphereModel = Resources.Load<GameObject>("Models/Sphere");
        GameObject cubeModel = Resources.Load<GameObject>("Models/Cube");

        assets.Add(new Asset("Sphere", "https://example.com/sphere", true, sphereModel));
        assets.Add(new Asset("Cube", "https://example.com/cube", false, cubeModel));
    }

    void PopulateAssetList()
    {
        foreach (Asset asset in assets)
        {
            GameObject button = Instantiate(assetButtonPrefab, contentPanel);
            button.GetComponentInChildren<Text>().text = asset.name;
            button.GetComponent<Button>().onClick.AddListener(() => OnAssetSelected(asset));
        }
    }

    void OnAssetSelected(Asset asset)
    {
        Debug.Log("Selected asset: " + asset.name);

        Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;
        GameObject instantiatedAsset = Instantiate(asset.model, spawnPosition, Quaternion.identity);
        instantiatedAsset.AddComponent<AssetBehavior>();
    }
}
