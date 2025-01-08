using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AssetManager : MonoBehaviour
{
    private List<Asset> assets = new List<Asset>();

    void Start()
    {
        // Load saved progress
        LoadAssets();
    }

    void SaveAssets()
    {
        // Save the asset list as a JSON string
        string json = JsonUtility.ToJson(new AssetListWrapper(assets));
        PlayerPrefs.SetString("AssetList", json);
        PlayerPrefs.Save();

        Debug.Log("Assets saved.");
    }

    void LoadAssets()
    {
        if (PlayerPrefs.HasKey("AssetList"))
        {
            string json = PlayerPrefs.GetString("AssetList");
            AssetListWrapper wrapper = JsonUtility.FromJson<AssetListWrapper>(json);
            assets = wrapper.assets;

            Debug.Log("Assets loaded: " + assets.Count);
        }
        else
        {
            Debug.Log("No saved progress found.");
        }
    }

    public void ResetAssets()
    {
        // Keep only permanent assets
        assets = assets.Where(asset => asset.permanent).ToList();

        // Clear all models in the scene
        foreach (GameObject model in GameObject.FindGameObjectsWithTag("Asset"))
        {
            Destroy(model);
        }

        // Reload permanent assets
        foreach (Asset asset in assets)
        {
            Instantiate(asset.model, Vector3.zero, Quaternion.identity);
        }

        SaveAssets();
    }

    [System.Serializable]
    public class AssetListWrapper
    {
        public List<Asset> assets;

        public AssetListWrapper(List<Asset> assets)
        {
            this.assets = assets;
        }
    }
}