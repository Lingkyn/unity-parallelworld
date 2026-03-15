using UnityEngine;

namespace ParallelWorld
{
    /// <summary>到达检查点时触发的反馈（UI 文案、音效、特效等）</summary>
    public interface ICheckpointFeedback
    {
        /// <summary>无参版本，兼容现有用法</summary>
        void ShowCheckpointReached();

        /// <summary>带检查点世界坐标，便于后续在位置播特效</summary>
        void ShowCheckpointReached(Vector3 worldPosition);
    }
}
