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

        /// <summary>
        /// 获取对象 XY 在光圈范围内的对象（用 FindObjectsByType，因 Collider 可能在光圈外被禁用）
        /// </summary>
        public static GameObject[] GetObjectsInApertureBounds(float minX, float maxX, float minY, float maxY, LayerMask layerMask)
        {
            var colliders = Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var result = new System.Collections.Generic.List<GameObject>();
            var seen = new System.Collections.Generic.HashSet<GameObject>();

            foreach (var c in colliders)
            {
                if (c == null || c.gameObject == null) continue;
                if (((1 << c.gameObject.layer) & layerMask) == 0) continue;
                if (seen.Contains(c.gameObject)) continue;

                Vector3 pos = c.transform.position;
                if (pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY)
                {
                    seen.Add(c.gameObject);
                    result.Add(c.gameObject);
                }
            }

            return result.ToArray();
        }
    }
}