using UnityEngine;

public class Heartbreak : MonoBehaviour
{
    public GameObject targetObject;

    private void OnTriggerEnter(Collider other)
    {
        if (targetObject == null)
        {
            return;
        }

        if (other.gameObject == targetObject || other.transform.root.gameObject == targetObject)
        {
            Destroy(gameObject);
        }
    }
}
