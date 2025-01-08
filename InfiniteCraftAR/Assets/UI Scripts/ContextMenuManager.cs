using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenuManager : MonoBehaviour
{
    public GameObject contextMenu; // Assign the context menu panel in the Inspector
    private GameObject selectedAsset; // The asset currently being interacted with

    public void OpenMenu(GameObject asset)
    {
        Debug.Log("Opening context menu for: " + asset.name);

        contextMenu.SetActive(true);
        contextMenu.transform.position = Camera.main.WorldToScreenPoint(asset.transform.position);
    }

    // Close the menu
    public void CloseMenu()
    {
        contextMenu.SetActive(false);
        selectedAsset = null;
    }

    // Delete the selected asset
    public void DeleteAsset()
    {
        if (selectedAsset != null)
        {
            Destroy(selectedAsset);
            CloseMenu();
        }
    }

    // Duplicate the selected asset
    public void DuplicateAsset()
    {
        if (selectedAsset != null)
        {
            Instantiate(selectedAsset, selectedAsset.transform.position + Vector3.right, Quaternion.identity);
            CloseMenu();
        }
    }
}