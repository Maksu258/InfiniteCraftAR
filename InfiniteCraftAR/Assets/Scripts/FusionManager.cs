using UnityEngine;
using UnityEngine.InputSystem;

public class FusionManager : MonoBehaviour
{
    public bool isFusionModeActive = false; // Tracks if Fusion Mode is active
    private GameObject firstSelectedObject;
    private GameObject secondSelectedObject;

    void Update()
    {
        if (isFusionModeActive)
        {
            // Check for left-click using the new Input System
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TrySelectObject();
            }
        }
    }

    public void ToggleFusionMode()
    {
        isFusionModeActive = !isFusionModeActive;

        if (isFusionModeActive)
        {
            Debug.Log("Fusion Mode Activated.");
            firstSelectedObject = null;
            secondSelectedObject = null;
        }
        else
        {
            Debug.Log("Fusion Mode Deactivated.");
        }
    }

    void TrySelectObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject selectedObject = hit.collider.gameObject;

            if (firstSelectedObject == null)
            {
                firstSelectedObject = selectedObject;
                Debug.Log("First Object Selected: " + selectedObject.name);
            }
            else if (secondSelectedObject == null && selectedObject != firstSelectedObject)
            {
                secondSelectedObject = selectedObject;
                Debug.Log("Second Object Selected: " + selectedObject.name);

                // Call placeholder fusion functions
                GenerateFusionWord(firstSelectedObject, secondSelectedObject);
                NewAsset(firstSelectedObject, secondSelectedObject);

                // Reset selections after fusion
                firstSelectedObject = null;
                secondSelectedObject = null;
            }
        }
    }

    void GenerateFusionWord(GameObject obj1, GameObject obj2)
    {
        Debug.Log($"Generating fusion word for {obj1.name} and {obj2.name}...");
        // Placeholder logic for generating a fusion word
    }

    void NewAsset(GameObject obj1, GameObject obj2)
    {
        Debug.Log($"Creating a new asset for {obj1.name} and {obj2.name}...");
        // Placeholder logic for creating a new fused asset
    }
}
