using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 房间/区域相机边界：玩家进入触发器时设置相机边界
    /// 可挂载于带 Collider(isTrigger) 的 GameObject，定义相机可移动范围
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CameraSwitch : MonoBehaviour
    {
        [Header("边界")]
        [SerializeField, Tooltip("该区域相机最小 XY（世界坐标）")]
        private Vector2 boundsMin = new Vector2(-10f, -10f);

        [SerializeField, Tooltip("该区域相机最大 XY（世界坐标）")]
        private Vector2 boundsMax = new Vector2(10f, 10f);

        [Header("引用")]
        [SerializeField, Tooltip("若不指定则从 Camera.main 获取")]
        private CameraController cameraController;

        private void Awake()
        {
            if (cameraController == null)
            {
                var cam = Camera.main;
                if (cam != null)
                    cameraController = cam.GetComponent<CameraController>();
            }

            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
                Debug.LogWarning("[CameraSwitch] Collider 应设置为 Is Trigger 以正确检测进入");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (cameraController == null) return;
            if (!IsPlayer(other)) return;

            cameraController.SetBounds(boundsMin, boundsMax);
        }

        /// <summary>
        /// 判断是否为玩家（可扩展：按 Tag、Layer 等）
        /// </summary>
        private static bool IsPlayer(Collider col)
        {
            return col.CompareTag("Player") || col.GetComponent<MovementController>() != null;
        }
    }
}
