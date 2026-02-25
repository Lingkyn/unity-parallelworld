using UnityEngine;

public class LightController : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public GameObject spotLightObject; // ⭐ 用 SetActive 控制这个

    [Header("Position Lock")]
    public float fixedZ = -18f;
    public float minY = 0.4f;

    [Header("Focus Lock")]
    public float focusY = 3f;

    private bool isLightOn = false;

    void Start()
    {
        // 默认关闭灯
        if (spotLightObject)
            spotLightObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (!mainCamera || !spotLightObject) return;

        // ⭐ 按 X 切换
        if (Input.GetKeyDown(KeyCode.X))
        {
            isLightOn = !isLightOn;
            spotLightObject.SetActive(isLightOn);
        }

        // ⭐⭐⭐ 位置始终更新（关键）⭐⭐⭐

        Vector3 mouse = Input.mousePosition;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(
                mouse.x,
                mouse.y,
                Mathf.Abs(mainCamera.transform.position.z - fixedZ)
            )
        );

        float clampedY = Mathf.Max(worldPos.y, minY);

        // 移动控制器（父物体）
        transform.position = new Vector3(worldPos.x, clampedY, fixedZ);

        // 聚焦点锁在 y = focusY
        Vector3 focusPoint = new Vector3(worldPos.x, focusY, 0f);
        transform.LookAt(focusPoint);
    }
}