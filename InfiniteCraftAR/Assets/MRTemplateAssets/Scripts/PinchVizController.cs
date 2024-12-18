using UnityEngine;


public class PinchVizController : MonoBehaviour
{
    [SerializeField]
    SkinnedMeshRenderer m_Pointer; // The visual pointer (blend shape controlled)

    UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor m_Interactor; // The ray interactor component

    // Start is called before the first frame update
    void Start()
    {
        // Get the XRRayInteractor component attached to this GameObject
        m_Interactor = this.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the interactor's "select" interaction is active
        float inputValue = m_Interactor.isSelectActive ? 1.0f : 0.0f;

        // Update the blend shape weight to visualize pinch strength
        m_Pointer.SetBlendShapeWeight(0, inputValue * 100f);
    }
}
