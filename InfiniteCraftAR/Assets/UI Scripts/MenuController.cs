using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;

    private bool isMenuOpen = false;

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        Debug.Log("AAAAAAAAAAAAAAAAAA");
        menuPanel.SetActive(isMenuOpen);
    }
}
