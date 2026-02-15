using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 相机规则与计算：目标位置、死区、方向预判、平滑插值、边界约束
    /// 不依赖 Unity 生命周期，仅包含纯规则与数值计算
    /// </summary>
    public class CameraSystem
    {
        private Vector3 _smoothVelocity;
        private Vector3 _lookAheadVelocity;

        /// <summary>
        /// 执行一帧相机逻辑：计算目标 → 应用死区 → 平滑插值 → 边界裁剪
        /// </summary>
        public CameraResult Tick(CameraInput input, CameraParams p)
        {
            Vector3 targetWorld = input.TargetPosition + p.TargetOffset;
            Vector3 delta = targetWorld - input.CurrentCameraPosition;

            // 死区：目标在死区内时相机不移动，超出时相机跟随使目标回到死区边缘
            float deadX = Mathf.Max(0.001f, p.DeadZoneX);
            float deadY = Mathf.Max(0.001f, p.DeadZoneY);
            float desiredX = Mathf.Abs(delta.x) < deadX
                ? input.CurrentCameraPosition.x
                : targetWorld.x - Mathf.Sign(delta.x) * deadX;
            float desiredY = Mathf.Abs(delta.y) < deadY
                ? input.CurrentCameraPosition.y
                : targetWorld.y - Mathf.Sign(delta.y) * deadY;
            Vector3 targetInZone = new Vector3(desiredX, desiredY, targetWorld.z);

            // 方向预判（Look Ahead）
            Vector3 lookAhead = ComputeLookAhead(input.TargetVelocity, p);
            Vector3 finalTarget = targetInZone + lookAhead;
            finalTarget.z = targetWorld.z;

            // SmoothDamp 插值
            Vector3 newPos = Vector3.SmoothDamp(
                input.CurrentCameraPosition,
                finalTarget,
                ref _smoothVelocity,
                p.SmoothTime,
                p.MaxFollowSpeed,
                input.DeltaTime
            );

            // 边界裁剪
            if (p.HasBounds)
            {
                newPos.x = Mathf.Clamp(newPos.x, p.BoundsMin.x, p.BoundsMax.x);
                newPos.y = Mathf.Clamp(newPos.y, p.BoundsMin.y, p.BoundsMax.y);
            }

            return new CameraResult
            {
                Position = newPos,
                SmoothVelocity = _smoothVelocity
            };
        }

        /// <summary>
        /// 根据速度计算预判偏移（空洞骑士风格：跳跃时相机略向上，移动时略向方向偏移）
        /// </summary>
        private Vector3 ComputeLookAhead(Vector3 velocity, CameraParams p)
        {
            float vx = velocity.x * p.LookAheadStrengthX;
            float vy = velocity.y * p.VerticalVelocityInfluence;
            return new Vector3(vx, vy, 0f);
        }
    }

    public struct CameraInput
    {
        /// <summary>跟随目标的世界位置（通常是角色脚底或中心）</summary>
        public Vector3 TargetPosition;
        /// <summary>目标当前速度（用于 Look Ahead）</summary>
        public Vector3 TargetVelocity;
        /// <summary>相机当前世界位置</summary>
        public Vector3 CurrentCameraPosition;
        /// <summary>时间步长</summary>
        public float DeltaTime;
    }

    public struct CameraParams
    {
        public float SmoothTime;
        public float MaxFollowSpeed;
        public float DeadZoneX;
        public float DeadZoneY;
        public float LookAheadStrengthX;
        public float VerticalVelocityInfluence;
        public Vector3 TargetOffset;
        public bool HasBounds;
        public Vector3 BoundsMin;
        public Vector3 BoundsMax;
    }

    public struct CameraResult
    {
        public Vector3 Position;
        public Vector3 SmoothVelocity;
    }

    /// <summary>
    /// 相机模式：用于不同房间/区域的切换
    /// </summary>
    public enum CameraMode
    {
        /// <summary>正常跟随，有死区和预判</summary>
        Follow,
        /// <summary>垂直方向锁定（如横版隧道）</summary>
        LockVertical,
        /// <summary>位置完全锁定（如过场）</summary>
        LockFull
    }
}
