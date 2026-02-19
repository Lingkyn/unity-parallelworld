using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 光圈核心逻辑：计算位置、缩放、维护开启状态
    /// 不依赖 Unity 生命周期，纯规则与数值计算
    /// </summary>
    public class ApertureCore
    {
        private float _scale;
        private bool _isActive;

        public bool IsActive => _isActive;
        public float Scale => _scale;

        public void Init(float initialScale, bool defaultActive)
        {
            _scale = initialScale;
            _isActive = defaultActive;
        }

        /// <summary>
        /// 根据滚轮增量更新缩放
        /// </summary>
        public void UpdateScale(float scrollDelta, float scaleSpeed, float minScale, float maxScale)
        {
            if (Mathf.Approximately(scrollDelta, 0f)) return;

            float delta = scrollDelta * scaleSpeed;
            _scale = Mathf.Clamp(_scale + delta, minScale, maxScale);
        }

        /// <summary>
        /// 切换开启状态
        /// </summary>
        public void Toggle()
        {
            _isActive = !_isActive;
        }

        /// <summary>
        /// 设置缩放（供 Controller 同步 Transform 后更新内部状态）
        /// </summary>
        public void SetScale(float scale)
        {
            _scale = scale;
        }
    }

    /// <summary>
    /// 范围检测：获取光圈 min/max XY，判断对象 XY 是否在范围内
    /// </summary>
    public static class RangeDetector
    {
        /// <summary>
        /// 光圈 XY 范围（中心 ± 半径）
        /// </summary>
        public static void GetApertureBounds(Vector3 center, float radius, out float minX, out float maxX, out float minY, out float maxY)
        {
            minX = center.x - radius;
            maxX = center.x + radius;
            minY = center.y - radius;
            maxY = center.y + radius;
        }

    }
}