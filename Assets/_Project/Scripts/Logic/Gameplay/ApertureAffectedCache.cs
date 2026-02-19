using System.Collections.Generic;
using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// ApertureAffected 层对象缓存：初始化时收集一次，避免每帧 FindObjectsByType
    /// </summary>
    public class ApertureAffectedCache
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private readonly List<GameObject> _result = new List<GameObject>();

        /// <summary>
        /// 缓存的对象数量
        /// </summary>
        public int Count => _objects.Count;

        /// <summary>
        /// 获取所有缓存对象（用于初始化时禁用 MeshRenderer/Collider 等）
        /// </summary>
        public IReadOnlyList<GameObject> All => _objects;

        /// <summary>
        /// 构建缓存：从场景中收集指定 Layer 上所有带 Collider 的 GameObject（去重）
        /// 应在 Awake/Start 调用一次
        /// </summary>
        public void Build(LayerMask layerMask)
        {
            _objects.Clear();
            var seen = new HashSet<GameObject>();

            var colliders = Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in colliders)
            {
                if (c == null || c.gameObject == null) continue;
                if (((1 << c.gameObject.layer) & layerMask) == 0) continue;
                if (seen.Contains(c.gameObject)) continue;

                seen.Add(c.gameObject);
                _objects.Add(c.gameObject);
            }
        }

        /// <summary>
        /// 从缓存中筛选 XY 在范围内的对象，复用内部 List 减少分配
        /// </summary>
        public List<GameObject> GetInBounds(float minX, float maxX, float minY, float maxY)
        {
            _result.Clear();
            foreach (var go in _objects)
            {
                if (go == null) continue;
                Vector3 p = go.transform.position;
                if (p.x >= minX && p.x <= maxX && p.y >= minY && p.y <= maxY)
                    _result.Add(go);
            }
            return _result;
        }
    }
}
