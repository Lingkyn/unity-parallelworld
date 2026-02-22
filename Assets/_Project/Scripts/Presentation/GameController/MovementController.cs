using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// Unity 壳：挂载于 Player，驱动 MovementSystem，将结果应用到 RealPlayer / ShadowPlayer 的 CharacterController
    /// 双躯体 XY 同步，Z 轴各自保持（ShadowPlayer 用于深度/层级）
    /// </summary>
    public class MovementController : MonoBehaviour
    {
        [SerializeField] private InputAdapter inputAdapter;
        [SerializeField] private MovementConfig config;
        [Header("躯体引用")]
        [SerializeField, Tooltip("实体玩家（含 CharacterController）")]
        private Transform realPlayer;
        [SerializeField, Tooltip("影子玩家（含 CharacterController）")]
        private Transform shadowPlayer;
        [SerializeField, Tooltip("ShadowPlayer 的 Z 轴偏移（用于深度）")]
        private float shadowZOffset = 10f;
        [Header("纸片翻转")]
        [SerializeField, Tooltip("需要随移动方向翻转的 Sprite（如 RealPlayer、ShadowPlayer 的 Sprite）")]
        private SpriteRenderer[] spriteRenderers;
        [SerializeField, Tooltip("水平输入小于此值时保持当前朝向")]
        private float flipThreshold = 0.1f;
        [SerializeField, Tooltip("Animator 列表：用于在翻转时触发 Flip 动画")]
        private Animator[] animators;

        private CharacterController _realController;
        private CharacterController _shadowController;
        private Transform _cameraTransform;
        private bool _hasCamera;
        private MovementSystem _movementSystem;
        private bool? _lastFaceRight;

        private void Awake()
        {
            if (realPlayer == null || shadowPlayer == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name.Equals("RealPlayer", System.StringComparison.OrdinalIgnoreCase))
                        realPlayer = child;
                    else if (child.name.Equals("ShadowPlayer", System.StringComparison.OrdinalIgnoreCase))
                        shadowPlayer = child;
                }
            }

            if (realPlayer != null)
                _realController = realPlayer.GetComponent<CharacterController>();
            if (shadowPlayer != null)
                _shadowController = shadowPlayer.GetComponent<CharacterController>();

            if (_realController == null)
                Debug.LogWarning("[MovementController] RealPlayer 未找到 CharacterController");
            if (_shadowController == null)
                Debug.LogWarning("[MovementController] ShadowPlayer 未找到 CharacterController");

            if (inputAdapter == null)
                inputAdapter = GetComponent<InputAdapter>();
            if (config == null)
                Debug.LogWarning("[MovementController] MovementConfig 未分配，将使用默认值");

            var cam = Camera.main;
            _hasCamera = cam != null;
            _cameraTransform = _hasCamera ? cam.transform : null;
            _movementSystem = new MovementSystem();
        }

        private void Update()
        {
            if (_realController == null || _shadowController == null) return;

            Transform activeTransform = realPlayer.gameObject.activeInHierarchy ? realPlayer : shadowPlayer;
            CharacterController activeController = activeTransform == realPlayer ? _realController : _shadowController;
            if (activeController == null || !activeController.gameObject.activeInHierarchy) return;

            float dt = Time.deltaTime;
            MovementParams p = BuildParams();

            Vector2 moveIntent = inputAdapter != null ? inputAdapter.GetMoveIntent() : Vector2.zero;
            bool jumpPressed = inputAdapter != null && inputAdapter.GetJumpPressed();
            bool isGrounded = activeController.isGrounded;

            Vector3 forward, right;
            GetForwardRight(out forward, out right);

            var input = new MovementInput
            {
                MoveIntent = moveIntent,
                JumpPressed = jumpPressed,
                IsGrounded = isGrounded,
                CurrentTime = Time.time,
                DeltaTime = dt,
                Forward = forward,
                Right = right
            };

            MovementResult result = _movementSystem.Tick(input, p);
            Vector3 velocity = result.Velocity * dt;

            activeController.Move(velocity);

            SyncXYAndParent(activeTransform);
            UpdateSpriteFlip(result.MoveDirection);
        }

        /// <summary>
        /// 同步父物体跟随当前 active 躯体，并更新 inactive 躯体的位置以便切换时正确
        /// </summary>
        private void SyncXYAndParent(Transform activeTransform)
        {
            if (realPlayer == null || shadowPlayer == null) return;

            Vector3 activePos = activeTransform.position;
            Transform inactive = activeTransform == realPlayer ? shadowPlayer : realPlayer;

            if (activeTransform == realPlayer)
                inactive.position = new Vector3(activePos.x, activePos.y, activePos.z + shadowZOffset);
            else
                inactive.position = new Vector3(activePos.x, activePos.y, activePos.z - shadowZOffset);

            transform.position = new Vector3(activePos.x, activePos.y, activeTransform == realPlayer ? activePos.z : activePos.z - shadowZOffset);
            realPlayer.localPosition = Vector3.zero;
            shadowPlayer.localPosition = new Vector3(0f, 0f, shadowZOffset);
        }

        private MovementParams BuildParams()
        {
            if (config == null)
                return new MovementParams { Speed = 6f, Gravity = 20f, JumpForce = 14f, AirControl = 0.3f, CoyoteTime = 0.15f, JumpBufferTime = 0.2f };

            return new MovementParams
            {
                Speed = config.speed,
                Gravity = config.gravity,
                JumpForce = config.jumpForce,
                AirControl = config.airControl,
                CoyoteTime = config.coyoteTime,
                JumpBufferTime = config.jumpBufferTime
            };
        }

        private void GetForwardRight(out Vector3 forward, out Vector3 right)
        {
            if (_hasCamera && _cameraTransform != null)
            {
                forward = _cameraTransform.forward;
                right = _cameraTransform.right;
            }
            else
            {
                forward = transform.forward;
                right = transform.right;
            }
        }

        private void UpdateSpriteFlip(Vector3 moveDir)
        {
            if (spriteRenderers == null || spriteRenderers.Length == 0) return;

            float horizontalInput;
            if (_hasCamera && _cameraTransform != null)
            {
                Vector3 right = _cameraTransform.right;
                right.y = 0f;
                right.Normalize();
                horizontalInput = Vector3.Dot(moveDir, right);
            }
            else
            {
                horizontalInput = moveDir.x;
            }

            if (Mathf.Abs(horizontalInput) <= flipThreshold) return;

            bool faceRight = horizontalInput < 0f;

            if (_lastFaceRight.HasValue && _lastFaceRight.Value != faceRight && animators != null)
            {
                foreach (var a in animators)
                    if (a != null) a.SetTrigger("Flip");
            }
            _lastFaceRight = faceRight;

            foreach (var sr in spriteRenderers)
            {
                if (sr != null)
                    sr.flipX = faceRight;
            }
        }
    }
}
