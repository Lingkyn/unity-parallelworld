using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 可交互物数据表：集中管理多处的提示文本
    /// 多处同类型物体可共享同一 entryId
    /// </summary>
    [CreateAssetMenu(fileName = "InteractableDatabase", menuName = "ParallelWorld/Interactable Database")]
    public class InteractableDatabase : ScriptableObject
    {
        [SerializeField] private List<InteractableEntry> _entries = new List<InteractableEntry>();

        private Dictionary<string, string> _lookup;

        /// <summary>
        /// 根据 id 查找提示文本，找不到返回 null
        /// </summary>
        public string GetPromptText(string entryId)
        {
            if (string.IsNullOrEmpty(entryId)) return null;

            if (_lookup == null)
                RebuildLookup();

            return _lookup != null && _lookup.TryGetValue(entryId, out string text) ? text : null;
        }

        private void RebuildLookup()
        {
            _lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in _entries)
            {
                if (string.IsNullOrEmpty(e.entryId)) continue;
                _lookup[e.entryId] = e.promptText ?? "";
            }
        }

        private void OnValidate()
        {
            _lookup = null;
        }
    }

    [Serializable]
    public class InteractableEntry
    {
        [Tooltip("唯一标识，如 bottle、door、npc_01")]
        public string entryId;

        [Tooltip("靠近时显示的提示文本")]
        public string promptText = "按【X】开启主光";
    }
}
