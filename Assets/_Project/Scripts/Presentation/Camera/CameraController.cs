using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// Unity 壳：挂载于 Camera，驱动 CameraSystem，将结果应用到 Transform
    /// 空洞骑士风格：平滑跟随、死区、方向预判、竖直速度响应
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("目标")]
        [SerializeField, Tooltip("跟随目标（通常为 Player 根节点）")]
        private Transform target;

        [Header("配置")]
        [SerializeField]
        private CameraConfig config;

        [Header("震动")]
        [SerializeField, Tooltip("可选：同物体上的 CameraShake 组件")]
        private CameraShake cameraShake;

        [Header("边界（可选）")]
        [SerializeField, Tooltip("是否启用边界约束")]
        private bool useBounds;

        [SerializeField]
        private Vector2 boundsMin = new Vector2(-50f, -50f);

        [SerializeField]
        private Vector2 boundsMax = new Vector2(50f, 50f);

        private CameraSystem _cameraSystem;
        private CharacterController _targetController;
        private Camera _camera;

        private void Awake()
        {
            _cameraSystem = new CameraSystem();
            _camera = GetComponent<Camera>();
            if (cameraShake == null)
                cameraShake = GetComponent<CameraShake>();

            if (target != null)
                _targetController = target.GetComponent<CharacterController>();

            if (config == null)
                Debug.LogWarning("[CameraController] CameraConfig 未分配，将使用默认值");
        }

        private void LateUpdate()
        {
            if (target == null) return;

            float dt = Time.deltaTime;
            CameraParams p = BuildParams();

            Vector3 targetPos = target.position;
            Vector3 targetVel = _targetController != null ? _targetController.velocity : Vector3.zero;

            var input = new CameraInput
            {
                TargetPosition = targetPos,
                TargetVelocity = targetVel,
                CurrentCameraPosition = transform.position,
                DeltaTime = dt
            };

            CameraResult result = _cameraSystem.Tick(input, p);
            Vector3 finalPos = result.Position;
            if (cameraShake != null)
                finalPos += cameraShake.GetShakeOffset();
            // 仅更新 XY，保持 Z 固定（相机在角色后方）
            transform.position = new Vector3(finalPos.x, finalPos.y, transform.position.z);
        }

        private CameraParams BuildParams()
        {
            if (config == null)
            {
                return new CameraParams
                {
                    SmoothTime = 0.25f,
                    MaxFollowSpeed = 30f,
                    DeadZoneX = 0.5f,
                    DeadZoneY = 0.5f,
                    LookAheadStrengthX = 0.5f,
                    VerticalVelocityInfluence = 0.15f,
                    TargetOffset = new Vector3(0f, 1f, 0f),
                    HasBounds = useBounds,
                    BoundsMin = new Vector3(boundsMin.x, boundsMin.y, -1000f),
                    BoundsMax = new Vector3(boundsMax.x, boundsMax.y, 1000f)
                };
            }

            return new CameraParams
            {
                SmoothTime = config.smoothTime,
                MaxFollowSpeed = config.maxFollowSpeed,
                DeadZoneX = config.deadZoneX,
                DeadZoneY = config.deadZoneY,
                LookAheadStrengthX = config.lookAheadStrengthX,
                VerticalVelocityInfluence = config.verticalVelocityInfluence,
                TargetOffset = config.targetOffset,
                HasBounds = useBounds,
                BoundsMin = new Vector3(boundsMin.x, boundsMin.y, -1000f),
                BoundsMax = new Vector3(boundsMax.x, boundsMax.y, 1000f)
            };
        }

        /// <summary>
        /// 设置跟随目标
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            _targetController = newTarget != null ? newTarget.GetComponent<CharacterController>() : null;
        }

        /// <summary>
        /// 设置边界（可由 CameraSwitch 调用）
        /// </summary>
        public void SetBounds(Vector2 min, Vector2 max)
        {
            useBounds = true;
            boundsMin = min;
            boundsMax = max;
        }

        /// <summary>
        /// 清除边界约束
        /// </summary>
        public void ClearBounds()
        {
            useBounds = false;
        }
    }
}
