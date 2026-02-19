using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 光圈系统的可配置参数
    /// </summary>
    [CreateAssetMenu(fileName = "ApertureConfig", menuName = "ParallelWorld/Aperture Config")]
    public class ApertureConfig : ScriptableObject
    {
        [Header("平面投射")]
        [Tooltip("光圈平面与相机的距离")]
        public float planeDistance = 10f;

        [Header("缩放")]
        [Tooltip("最小缩放")]
        public float minScale = 1f;

        [Tooltip("最大缩放")]
        public float maxScale = 10f;

        [Tooltip("滚轮缩放速度")]
        public float scaleSpeed = 2f;

        [Header("状态")]
        [Tooltip("初始是否开启")]
        public bool defaultActive = true;
    }
}