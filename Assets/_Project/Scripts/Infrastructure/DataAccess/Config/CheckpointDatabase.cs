using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParallelWorld
{
    [Serializable]
    public class CheckpointEntry
    {
        [Tooltip("检查点唯一标识，参与存档与读档恢复")]
        public string checkpointId;
        [Tooltip("同场景内排序，升序最小者可作为默认出生点")]
        public int order;
    }

    /// <summary>
    /// 检查点表：集中管理各场景检查点 ID 与排序，RespawnPointDetector 通过表索引引用。
    /// </summary>
    [CreateAssetMenu(fileName = "CheckpointDatabase", menuName = "ParallelWorld/Checkpoint Database")]
    public class CheckpointDatabase : ScriptableObject
    {
        [SerializeField] private List<CheckpointEntry> _entries = new List<CheckpointEntry>();

        /// <summary>根据表索引取检查点 ID，越界或空返回 null</summary>
        public string GetCheckpointId(int index)
        {
            var entry = GetEntry(index);
            return entry != null && !string.IsNullOrEmpty(entry.checkpointId) ? entry.checkpointId : null;
        }

        /// <summary>根据表索引取条目，越界返回 null</summary>
        public CheckpointEntry GetEntry(int index)
        {
            if (index < 0 || index >= _entries.Count) return null;
            return _entries[index];
        }

        public int EntryCount => _entries?.Count ?? 0;
    }
}
