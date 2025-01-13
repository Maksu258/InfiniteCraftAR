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