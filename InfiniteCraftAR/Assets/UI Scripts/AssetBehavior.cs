using UnityEngine;

public class AssetBehavior : MonoBehaviour
{
    void OnMouseDown()
    {
        ContextMenuManager menuManager = FindObjectOfType<ContextMenuManager>();
        if (menuManager != null)
        {
            menuManager.OpenMenu(gameObject);
        }
    }
}
