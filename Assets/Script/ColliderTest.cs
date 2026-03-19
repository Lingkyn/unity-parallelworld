using UnityEngine;

public class ColliderTest : MonoBehaviour
{
    public GameObject panel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Main Car")
        {
            panel.SetActive(true);
            Invoke(nameof(HidePanel), 2f); //Time
        }
    }

    private void HidePanel() //Hide UI
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}
