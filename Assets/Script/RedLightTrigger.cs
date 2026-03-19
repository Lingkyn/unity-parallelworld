using UnityEngine;

public class RedLightTrigger : MonoBehaviour
{
    public TrafficLightController trafficLight;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Main Car")
        {
            trafficLight.StartRedLight(); // Trigger red
        }
    }
}
