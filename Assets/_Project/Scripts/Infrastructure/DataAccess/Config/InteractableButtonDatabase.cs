using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 按钮型可交互物数据表：entryId 优先级顺序、buttonText、animationClip
    /// 同距离时按表序决定显示哪个按钮
    /// </summary>
    [CreateAssetMenu(fileName = "InteractableButtonDatabase", menuName = "ParallelWorld/Interactable Button Database")]
    public class InteractableButtonDatabase : ScriptableObject
    {
        [SerializeField] private List<InteractableButtonEntry> _entries = new List<InteractableButtonEntry>();

        private Dictionary<string, int> _priorityLookup;
        private Dictionary<string, InteractableButtonEntry> _entryLookup;

        /// <summary>
        /// 返回 entryId 在表中的索引，越小越优先；未找到返回 int.MaxValue
        /// </summary>
        public int GetPriority(string entryId)
        {
            if (string.IsNullOrEmpty(entryId)) return int.MaxValue;

            if (_priorityLookup == null)
                RebuildLookup();

            return _priorityLookup != null && _priorityLookup.TryGetValue(entryId, out int p) ? p : int.MaxValue;
        }

        /// <summary>
        /// 按 entryId 查表，供表模式时获取 buttonText、animationClip
        /// </summary>
        public InteractableButtonEntry GetEntry(string entryId)
        {
            if (string.IsNullOrEmpty(entryId)) return null;

            if (_entryLookup == null)
                RebuildLookup();

            return _entryLookup != null && _entryLookup.TryGetValue(entryId, out var e) ? e : null;
        }

        private void RebuildLookup()
        {
            _priorityLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _entryLookup = new Dictionary<string, InteractableButtonEntry>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                if (string.IsNullOrEmpty(e.entryId)) continue;
                _priorityLookup[e.entryId] = i;
                _entryLookup[e.entryId] = e;
            }
        }

        private void OnValidate()
        {
            _priorityLookup = null;
            _entryLookup = null;
        }
    }

    [Serializable]
    public class InteractableButtonEntry
    {
        [Tooltip("唯一标识，与 InteractableButtonData 组件上的 entryId 对应")]
        public string entryId;

        [Tooltip("按钮显示文本，若不填则用组件本地配置")]
        public string buttonText;

        [Tooltip("AnimationClip 引用，多处可共享")]
        public AnimationClip animationClip;

        [Tooltip("场景加载时强制进入的 Animator 状态名，用于避免默认状态导致一进场景就播放。留空则不强制")]
        public string defaultStateName;
    }
}
