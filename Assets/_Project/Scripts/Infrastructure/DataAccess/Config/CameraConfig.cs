using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 相机系统的可配置参数（空洞骑士风格）
    /// </summary>
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "ParallelWorld/Camera Config")]
    public class CameraConfig : ScriptableObject
    {
        [Header("平滑跟随")]
        [Tooltip("相机平滑跟随目标的时间（秒），越小越灵敏")]
        public float smoothTime = 0.25f;

        [Tooltip("最大相机跟随速度（防止剧烈运动时跟不上）")]
        public float maxFollowSpeed = 30f;

        [Header("死区")]
        [Tooltip("目标在死区内不触发相机移动（X 半宽）")]
        public float deadZoneX = 0.5f;

        [Tooltip("目标在死区内不触发相机移动（Y 半高）")]
        public float deadZoneY = 0.5f;

        [Header("方向预判（Look Ahead）")]
        [Tooltip("水平方向预判强度（根据速度向移动方向偏移）")]
        [Range(0f, 2f)]
        public float lookAheadStrengthX = 0.5f;

        [Tooltip("竖直速度对相机 Y 偏移的影响（跳跃时相机略向上）")]
        [Range(0f, 1f)]
        public float verticalVelocityInfluence = 0.15f;

        [Tooltip("预判变化速率（越大越灵敏）")]
        public float lookAheadSmoothTime = 0.1f;

        [Header("相机偏移")]
        [Tooltip("相机相对目标的默认偏移（如 Y 略高于脚底）")]
        public Vector3 targetOffset = new Vector3(0f, 1f, 0f);

        [Header("正交尺寸（2D 使用）")]
        [Tooltip("正交相机的 Size")]
        public float orthographicSize = 5f;
    }
}