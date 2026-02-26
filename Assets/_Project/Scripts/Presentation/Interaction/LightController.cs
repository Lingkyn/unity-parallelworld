using UnityEngine;

public class LightController : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public GameObject spotLightObject; 

    [Header("Position Lock")]
    public float fixedZ = -18f;
    public float minY = 0.4f;

    [Header("Focus Lock")]
    public float focusY = 3f;

    private bool isLightOn = false;

    void Start()
    {
        
        if (spotLightObject)
            spotLightObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (!mainCamera || !spotLightObject) return;

        
        if (Input.GetKeyDown(KeyCode.X))
        {
            isLightOn = !isLightOn;
            spotLightObject.SetActive(isLightOn);
        }

        

        Vector3 mouse = Input.mousePosition;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(
                mouse.x,
                mouse.y,
                Mathf.Abs(mainCamera.transform.position.z - fixedZ)
            )
        );

        float clampedY = Mathf.Max(worldPos.y, minY);

        
        transform.position = new Vector3(worldPos.x, clampedY, fixedZ);

        
        Vector3 focusPoint = new Vector3(worldPos.x, focusY, 0f);
        transform.LookAt(focusPoint);
    }
}