using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 角色移动控制器：根据输入意图驱动 3D 玩家位移与跳跃
    /// 执行顺序：读意图 → 更地面状态 → 算竖直速度 → 算水平速度 → Move
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

        // 状态
        private Vector3 _velocity;
        private bool _jumpRequested;
        private float _lastGroundedTime;
        private float _lastJumpRequestTime;
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

        }

        private void Update()
        {
            float dt = Time.deltaTime;
            float speed = config != null ? config.speed : 6f;
            float gravity = config != null ? config.gravity : 20f;
            float jumpForce = config != null ? config.jumpForce : 14f;
            float airControl = config != null ? config.airControl : 0.3f;
            float coyoteTime = config != null ? config.coyoteTime : 0.15f;
            float jumpBufferTime = config != null ? config.jumpBufferTime : 0.2f;

            // 1. 读意图
            Vector2 moveIntent = inputAdapter != null ? inputAdapter.GetMoveIntent() : Vector2.zero;
            if (inputAdapter != null && inputAdapter.GetJumpPressed())
            {
                _jumpRequested = true;
                _lastJumpRequestTime = Time.time;
            }

            // 2. 更地面状态
            bool isGrounded = _controller.isGrounded;
            if (isGrounded)
                _lastGroundedTime = Time.time;

            // 3. 算竖直速度
            bool canCoyoteJump = (Time.time - _lastGroundedTime) <= coyoteTime;
            bool canBufferedJump = (Time.time - _lastJumpRequestTime) <= jumpBufferTime;

            if (isGrounded && _velocity.y < 0f)
                _velocity.y = -2f; // 贴地小负值，避免浮空

            if (_jumpRequested && (isGrounded || canCoyoteJump))
            {
                _velocity.y = jumpForce;
                _jumpRequested = false;
            }
            else if (!isGrounded)
            {
                _velocity.y -= gravity * dt;
                if (!canBufferedJump)
                    _jumpRequested = false;
            }

            // 4. 算水平速度
            Vector3 moveDir = GetMoveDirection(moveIntent);
            float controlFactor = isGrounded ? 1f : airControl;
            Vector3 horizontalVelocity = moveDir * (speed * controlFactor);

            _velocity.x = horizontalVelocity.x;
            _velocity.z = horizontalVelocity.z;

            // 纸片 X 翻转：根据水平移动方向翻转 Sprite
            UpdateSpriteFlip(moveDir);

            // 5. Move
            _controller.Move(_velocity * dt);
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

            // 方向变了就触发翻转
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

        private Vector3 GetMoveDirection(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f) return Vector3.zero;

            Vector3 forward;
            Vector3 right;

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

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * input.y + right * input.x).normalized;
        }
    }
}
