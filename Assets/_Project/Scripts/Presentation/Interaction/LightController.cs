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
    
    [Header("Level Rule")]
    [SerializeField] private bool lockLightState = false;
    [SerializeField] private bool lockedLightOn = true;
    [SerializeField] private bool initialLightOn = false;

    public static System.Action<bool> OnLightToggle;

    private bool isLightOn = false;

    void Start()
    {
        ApplyLightState(lockLightState ? lockedLightOn : initialLightOn);
    }

    void LateUpdate()
    {
        if (!mainCamera || !spotLightObject) return;

        
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (lockLightState)
                return;

            isLightOn = !isLightOn;
            ApplyLightState(isLightOn);
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

    private void ApplyLightState(bool turnOn)
    {
        isLightOn = turnOn;
        if (spotLightObject)
            spotLightObject.SetActive(isLightOn);

            OnLightToggle?.Invoke(isLightOn);
    }
}