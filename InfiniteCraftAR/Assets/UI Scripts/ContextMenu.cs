using UnityEngine;
using UnityEngine.UI;

public class ContextMenu : MonoBehaviour
{
    public void Initialize(GameObject target)
    {
        transform.Find("DuplicateButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            Instantiate(target, target.transform.position + Vector3.right, Quaternion.identity);
        });

        transform.Find("DeleteButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            Destroy(target);
            Destroy(gameObject);
        });
    }
}
