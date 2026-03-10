using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 死亡复活逻辑：记录当前复活点，计算复活目标位置。纯逻辑，无 Unity 依赖。
    /// </summary>
    public class DeathRespawnSystem
    {
        private Vector3 _currentRespawnPosition;
        private bool _hasActivatedRespawnPoint;
        private string _currentCheckpointId;

        /// <summary>触发死亡流程，返回复活目标位置</summary>
        public Vector3 NotifyDeath()
        {
            return GetRespawnPosition();
        }

        /// <summary>激活该复活点，记录为当前复活点</summary>
        /// <param name="position">复活点位置</param>
        /// <param name="checkpointId">供存档/检查点扩展，可空</param>
        public void NotifyActivateRespawnPoint(Vector3 position, string checkpointId = null)
        {
            _currentRespawnPosition = position;
            _hasActivatedRespawnPoint = true;
            _currentCheckpointId = checkpointId;
        }

        private Vector3 _defaultSpawnPosition;

        /// <summary>获取当前复活点位置，若无激活则返回默认出生点</summary>
        public Vector3 GetRespawnPosition()
        {
            return _hasActivatedRespawnPoint ? _currentRespawnPosition : _defaultSpawnPosition;
        }

        /// <summary>设置默认出生点（场景加载时由 Controller 传入 Config 的 defaultSpawnPosition）</summary>
        public void SetDefaultSpawnPosition(Vector3 position)
        {
            _defaultSpawnPosition = position;
        }

        /// <summary>供存档/检查点：读取当前检查点 ID</summary>
        public string GetCurrentCheckpointId()
        {
            return _currentCheckpointId;
        }

        /// <summary>供存档/检查点：场景加载时恢复检查点</summary>
        public void SetCurrentCheckpoint(string checkpointId)
        {
            _currentCheckpointId = checkpointId;
        }
    }
}
