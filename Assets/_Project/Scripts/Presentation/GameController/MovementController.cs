using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// Unity 壳：挂载于 GameObject，驱动 MovementSystem，将结果应用到 CharacterController / Sprite / Animator
    /// 不写规则，不保存核心状态，仅作为生命周期入口与组件适配层
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class MovementController : MonoBehaviour
    {
        [SerializeField] private InputAdapter inputAdapter;
        [SerializeField] private MovementConfig config;
        [Header("纸片翻转")]
        [SerializeField, Tooltip("需要随移动方向翻转的 Sprite（如 RealPlayer、ShadowPlayer 的 Sprite）")]
        private SpriteRenderer[] spriteRenderers;
        [SerializeField, Tooltip("水平输入小于此值时保持当前朝向")]
        private float flipThreshold = 0.1f;
        [SerializeField, Tooltip("Animator 列表：用于在翻转时触发 Flip 动画")]
        private Animator[] animators;

        private CharacterController _controller;
        private Transform _cameraTransform;
        private bool _hasCamera;
        private MovementSystem _movementSystem;
        private bool? _lastFaceRight;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
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
            float dt = Time.deltaTime;
            MovementParams p = BuildParams();

            Vector2 moveIntent = inputAdapter != null ? inputAdapter.GetMoveIntent() : Vector2.zero;
            bool jumpPressed = inputAdapter != null && inputAdapter.GetJumpPressed();
            bool isGrounded = _controller.isGrounded;

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

            _controller.Move(result.Velocity * dt);
            UpdateSpriteFlip(result.MoveDirection);
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
