using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBehavior : MonoBehaviour
{
    void OnMouseDown()
    {
        Debug.Log("Clicked on: " + gameObject.name);

        ContextMenuManager menuManager = FindObjectOfType<ContextMenuManager>();
        if (menuManager != null)
        {
            Debug.Log("Opening context menu...");
            menuManager.OpenMenu(gameObject);
        }
    }
}
    
