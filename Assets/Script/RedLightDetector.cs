using UnityEngine;

public class RedLightDetector : MonoBehaviour
{
    public TrafficLightController trafficLight;
    public GameObject warningPanel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Main Car")
        {
            // Check red light
            if (trafficLight.IsRedLight())
            {
                warningPanel.SetActive(true);
                Invoke("CloseWarning", 3f); // Auto close
            }
        }
    }
    // Hide UI
    void CloseWarning()
    {
        warningPanel.SetActive(false);
    }
}
