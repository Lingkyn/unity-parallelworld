using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 移动规则与计算：处理速度、跳跃、Coyote Time、Jump Buffer 等逻辑
    /// 不依赖 Unity 生命周期，仅包含纯规则与数值计算
    /// </summary>
    public class MovementSystem
    {
        private Vector3 _velocity;
        private bool _jumpRequested;
        private float _lastGroundedTime;
        private float _lastJumpRequestTime;

        /// <summary>
        /// 执行一帧移动逻辑：读意图 → 更地面状态 → 算竖直速度 → 算水平速度
        /// </summary>
        public MovementResult Tick(MovementInput input, MovementParams p)
        {
            if (input.JumpPressed)
            {
                _jumpRequested = true;
                _lastJumpRequestTime = input.CurrentTime;
            }

            bool isGrounded = input.IsGrounded;
            if (isGrounded)
                _lastGroundedTime = input.CurrentTime;

            bool canCoyoteJump = (input.CurrentTime - _lastGroundedTime) <= p.CoyoteTime;
            bool canBufferedJump = (input.CurrentTime - _lastJumpRequestTime) <= p.JumpBufferTime;

            // 竖直速度
            if (isGrounded && _velocity.y < 0f)
                _velocity.y = -2f;

            if (_jumpRequested && (isGrounded || canCoyoteJump))
            {
                _velocity.y = p.JumpForce;
                _jumpRequested = false;
            }
            else if (!isGrounded)
            {
                _velocity.y -= p.Gravity * input.DeltaTime;
                if (!canBufferedJump)
                    _jumpRequested = false;
            }

            // 水平速度
            Vector3 moveDir = GetMoveDirection(input.MoveIntent, input.Forward, input.Right);
            float controlFactor = isGrounded ? 1f : p.AirControl;
            Vector3 horizontalVelocity = moveDir * (p.Speed * controlFactor);

            _velocity.x = horizontalVelocity.x;
            _velocity.z = horizontalVelocity.z;

            return new MovementResult
            {
                Velocity = _velocity,
                MoveDirection = moveDir
            };
        }

        /// <summary>
        /// 将 2D 输入转换为世界空间移动方向（纯数学，无 Unity 依赖）
        /// </summary>
        public static Vector3 GetMoveDirection(Vector2 input, Vector3 forward, Vector3 right)
        {
            if (input.sqrMagnitude < 0.01f) return Vector3.zero;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * input.y + right * input.x).normalized;
        }
    }

    public struct MovementInput
    {
        public Vector2 MoveIntent;
        public bool JumpPressed;
        public bool IsGrounded;
        public float CurrentTime;
        public float DeltaTime;
        public Vector3 Forward;
        public Vector3 Right;
    }

    public struct MovementParams
    {
        public float Speed;
        public float Gravity;
        public float JumpForce;
        public float AirControl;
        public float CoyoteTime;
        public float JumpBufferTime;
    }

    public struct MovementResult
    {
        public Vector3 Velocity;
        public Vector3 MoveDirection;
    }
}
