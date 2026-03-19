using UnityEngine;

public class SpeedChecker : MonoBehaviour
{
    public Rigidbody carRb;
    public float speedLimit = 30f;    // limit
    public GameObject warningPanel;   //  UI 

    void Update()
    {
        if (carRb == null) return;

        float speed = carRb.linearVelocity.magnitude * 3.6f;

        if (speed > speedLimit)
        {
            warningPanel.SetActive(true);
        }
        else
        {
            warningPanel.SetActive(false);
        }
    }
}
