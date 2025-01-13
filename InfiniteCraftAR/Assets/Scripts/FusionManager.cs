using UnityEngine;
using UnityEngine.InputSystem;

public class FusionManager : MonoBehaviour
{
    public bool isFusionModeActive = false;
    private GameObject firstSelectedObject;
    private GameObject secondSelectedObject;
    private APIManager apiManager;

    public Camera mainCamera; // Assign manually if using a custom camera setup

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Fallback to Main Camera
        }

        apiManager = FindObjectOfType<APIManager>();
        if (apiManager == null)
        {
            Debug.LogError("APIManager not found in the scene!");
        }
    }

    void Update()
    {
        if (isFusionModeActive)
        {
            if (Mouse.current == null)
            {
                Debug.LogError("Mouse input not detected! Check Input System setup.");
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TrySelectObject();
            }
        }
    }

    void TrySelectObject()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not assigned or tagged as MainCamera.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider == null)
            {
                Debug.LogError("Raycast hit an object without a collider!");
                return;
            }

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

                // Start the API call via APIManager
                string[] words = { firstSelectedObject.name, secondSelectedObject.name };
                Debug.Log(words);
                StartCoroutine(apiManager.generateFusionWord(apiManager.apiUrl, words));
              
                // Reset selections
                firstSelectedObject = null;
                secondSelectedObject = null;
            }
        }
        else
        {
            Debug.Log("Raycast did not hit any object.");
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
}
