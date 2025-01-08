using UnityEngine;

public class ContextMenuManager : MonoBehaviour
{
    public GameObject contextMenuPrefab;
    private GameObject activeMenu;

    public void OpenMenu(GameObject target)
    {
        if (activeMenu != null) Destroy(activeMenu);

        activeMenu = Instantiate(contextMenuPrefab, Input.mousePosition, Quaternion.identity, transform);
        activeMenu.GetComponent<ContextMenu>().Initialize(target);
    }
}
