using UnityEngine;

public class TriggerTest : MonoBehaviour
{
    public GameObject panel;

    private void OnTriggerEnter(Collider other) //Show UI
    {
        if (other.name == "Main Car") 
        {
            panel.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other) //Hide UI
    {
        if (other.name == "Main Car")
        {
            panel.SetActive(false);
        }
    }
}

