using UnityEngine;
using UnityEngine.UI;

public class TrafficLightController : MonoBehaviour
{
    public GameObject redLight;
    public GameObject greenLight;
    public GameObject warningUI;

    public float redTime = 15f;
    private float timer;
    private bool startCount = false;

    private void Update()
    {
        if (startCount)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                redLight.SetActive(false);  // Red off
                greenLight.SetActive(true); // Green on

                warningUI.SetActive(false); // Hide warning

                startCount = false;
            }
        }
    }
    public void StartRedLight()
    {
        warningUI.SetActive(true); // Show warning

        redLight.SetActive(true);  // Red on
        greenLight.SetActive(false); // Green off

        timer = redTime;
        startCount = true;
    }

    public bool IsRedLight()
    {
        return redLight.activeSelf;
    }
}
