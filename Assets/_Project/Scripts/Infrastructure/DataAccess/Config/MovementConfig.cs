using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 角色移动控制器的可配置参数
    /// </summary>
    [CreateAssetMenu(fileName = "MovementConfig", menuName = "ParallelWorld/Movement Config")]
    public class MovementConfig : ScriptableObject
    {
        [Header("移动")]
        [Tooltip("地面移动速度")]
        public float speed = 8f;

        [Header("平面约束")]
        [Tooltip("开启后将 RealPlayer 的世界坐标 Z 固定为 realFixedZ")]
        public bool lockRealZAxis = false;

        [Tooltip("当 lockRealZAxis 开启时生效：RealPlayer 的固定世界坐标 Z")]
        public float realFixedZ = 0f;

        [Tooltip("开启后将 ShadowPlayer 的世界坐标 Z 固定为 shadowFixedZ")]
        public bool lockShadowZAxis = false;

        [Tooltip("当 lockShadowZAxis 开启时生效：ShadowPlayer 的固定世界坐标 Z")]
        public float shadowFixedZ = 5f;

        [Header("重力与跳跃")]
        [Tooltip("重力加速度")]
        public float gravity = 20f;

        [Tooltip("跳跃初速度")]
        public float jumpForce = 10f;

        [Tooltip("空中可改变方向的程度 (0~1)")]
        [Range(0f, 1f)]
        public float airControl = 0.75f;

        [Header("Coyote Time")]
        [Tooltip("离地后仍可跳跃的时长(秒)")]
        public float coyoteTime = 0.15f;

        [Header("Jump Buffer")]
        [Tooltip("落地前按下跳跃会缓冲的时长(秒)")]
        public float jumpBufferTime = 0.2f;
    }
}
