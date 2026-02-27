using System.Collections.Generic;
using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 可交互物缓存：按 Tag 收集，避免每帧 Find
    /// </summary>
    public class InteractableCache
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private readonly HashSet<GameObject> _seen = new HashSet<GameObject>();

        public int Count => _objects.Count;
        public IReadOnlyList<GameObject> All => _objects;

        /// <summary>
        /// 按 Tag 收集场景中可交互物
        /// </summary>
        public void Build(string interactableTag)
        {
            _objects.Clear();
            _seen.Clear();

            if (string.IsNullOrEmpty(interactableTag)) return;

            var colliders = Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var c in colliders)
            {
                if (c == null || c.gameObject == null) continue;
                if (!c.gameObject.CompareTag(interactableTag)) continue;
                if (_seen.Contains(c.gameObject)) continue;

                _seen.Add(c.gameObject);
                _objects.Add(c.gameObject);
            }
        }

        /// <summary>
        /// 获取提示文本（优先 InteractableData 表/本地，否则 IInteractable，否则 defaultText）
        /// 推荐使用 InteractableData.ResolvePromptText(go, config) 以支持数据表
        /// </summary>
        public static string GetPromptText(GameObject go, string defaultText)
        {
            if (go == null) return defaultText;

            var interactable = go.GetComponent<IInteractable>();
            if (interactable != null)
                return interactable.GetPromptText();

            return defaultText;
        }
    }
}
