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

        /// <summary>激活该复活点，记录为当前复活点。若与当前复活点一致则视为重复激活。</summary>
        /// <param name="position">复活点位置</param>
        /// <param name="checkpointId">供存档/检查点扩展，可空</param>
        /// <returns>true 表示本次为新激活（状态有变化），false 表示重复经过同一检查点</returns>
        public bool NotifyActivateRespawnPoint(Vector3 position, string checkpointId = null)
        {
            bool positionSame = (position - _currentRespawnPosition).sqrMagnitude < 1e-5f;
            bool idSame = string.Equals(_currentCheckpointId, checkpointId);
            if (_hasActivatedRespawnPoint && positionSame && idSame)
                return false;

            _currentRespawnPosition = position;
            _hasActivatedRespawnPoint = true;
            _currentCheckpointId = checkpointId;
            return true;
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

        /// <summary>供存档/检查点：读取当前检查点位置（供存档写入）</summary>
        public Vector3 GetCurrentCheckpointPosition()
        {
            return _hasActivatedRespawnPoint ? _currentRespawnPosition : _defaultSpawnPosition;
        }

        /// <summary>供存档/检查点：场景加载时恢复检查点（设置位置与 ID，并标记已激活）</summary>
        public void SetRespawnFromCheckpoint(Vector3 position, string checkpointId)
        {
            _currentRespawnPosition = position;
            _hasActivatedRespawnPoint = true;
            _currentCheckpointId = checkpointId;
        }

        /// <summary>供存档/检查点：仅设置 ID（旧接口，保留兼容）</summary>
        public void SetCurrentCheckpoint(string checkpointId)
        {
            _currentCheckpointId = checkpointId;
        }
    }
}
