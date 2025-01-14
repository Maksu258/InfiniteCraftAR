using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FusionManager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject fusionButton; // Button to toggle Fusion Mode
    public GameObject fusionScrollView; // Scroll View for asset selection
    public Transform contentPanel; // Content container for buttons
    public GameObject assetButtonPrefab; // Prefab for asset buttons
    public APIManager apiManager; // Reference to the APIManager

    private bool isFusionModeActive = false;
    private Asset firstSelectedAsset = null;
    private Asset secondSelectedAsset = null;

    void Start()
    {
        // Set up the Fusion Button
        if (fusionButton != null)
        {
            fusionButton.GetComponent<Button>().onClick.AddListener(ToggleFusionMode);
        }

        // Ensure the Scroll View is initially hidden
        if (fusionScrollView != null)
        {
            fusionScrollView.SetActive(false);
        }
    }

    // Toggle Fusion Mode
    public void ToggleFusionMode()
    {
        isFusionModeActive = !isFusionModeActive;
        fusionScrollView.SetActive(isFusionModeActive);

        if (isFusionModeActive)
        {
            Debug.Log("Fusion Mode Activated.");
            firstSelectedAsset = null;
            secondSelectedAsset = null;
            PopulateFusionScrollView();
        }
        else
        {
            Debug.Log("Fusion Mode Deactivated.");
        }
    }

    // Populate the Scroll View with asset buttons
    private void PopulateFusionScrollView()
    {
        // Clear existing buttons
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Get assets from AssetListManager
        AssetListManager assetListManager = FindObjectOfType<AssetListManager>();
        if (assetListManager == null)
        {
            Debug.LogError("AssetListManager not found in the scene.");
            return;
        }

        foreach (Asset asset in assetListManager.GetAssets())
        {
            GameObject button = Instantiate(assetButtonPrefab, contentPanel);
            button.transform.localScale = Vector3.one;

            // Set button text
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = asset.name;
            }

            // Add click listener
            button.GetComponent<Button>().onClick.AddListener(() => OnAssetSelected(asset));
        }
    }

    // Handle asset selection in Fusion Mode
    private void OnAssetSelected(Asset asset)
    {
        if (firstSelectedAsset == null)
        {
            firstSelectedAsset = asset;
            Debug.Log($"First Asset Selected: {asset.name}");
        }
        else if (secondSelectedAsset == null && asset != firstSelectedAsset)
        {
            secondSelectedAsset = asset;
            Debug.Log($"Second Asset Selected: {asset.name}");

            // Both assets selected, initiate fusion process
            StartFusionProcess();
        }
    }

    // Start the fusion process by calling the API
    private void StartFusionProcess()
    {
        if (firstSelectedAsset != null && secondSelectedAsset != null)
        {
            string[] fusionWords = { firstSelectedAsset.name, secondSelectedAsset.name };
            Debug.Log($"Initiating Fusion for: {fusionWords[0]} and {fusionWords[1]}");

            // Call APIManager to generate the fusion object
            StartCoroutine(apiManager.generateFusionWord(apiManager.apiUrl, fusionWords));

            // Reset after fusion
            firstSelectedAsset = null;
            secondSelectedAsset = null;

            // Optionally exit Fusion Mode after initiating fusion
            ToggleFusionMode();
        }
        else
        {
            Debug.LogError("Both assets must be selected before starting the fusion process.");
        }
    }
}
