namespace ParallelWorld
{
    public enum SpawnSystemState
    {
        Idle,
        Active,
        Paused,
        Stopped,
        Cleared
    }

    /// <summary>
    /// 对象生成纯逻辑：控制状态、计数与是否允许继续生成。
    /// </summary>
    public class ObjectSpawnSystem
    {
        private int _maxAliveCount = 1;
        private bool _loopSpawn = true;
        private int _aliveCount;
        private bool _hasSpawnedOnce;

        public SpawnSystemState State { get; private set; } = SpawnSystemState.Idle;
        public int AliveCount => _aliveCount;

        public void Configure(SpawnRule rule)
        {
            if (rule == null)
                return;

            _maxAliveCount = rule.maxAliveCount < 1 ? 1 : rule.maxAliveCount;
            _loopSpawn = rule.loopSpawn;
        }

        public void Activate()
        {
            if (State == SpawnSystemState.Stopped)
                return;

            if (State == SpawnSystemState.Paused || State == SpawnSystemState.Idle || State == SpawnSystemState.Cleared)
                State = SpawnSystemState.Active;
        }

        public void Pause()
        {
            if (State == SpawnSystemState.Active)
                State = SpawnSystemState.Paused;
        }

        public void Resume()
        {
            if (State == SpawnSystemState.Paused)
                State = SpawnSystemState.Active;
        }

        public void Stop()
        {
            State = SpawnSystemState.Stopped;
        }

        public void Deactivate()
        {
            if (State == SpawnSystemState.Active)
                State = SpawnSystemState.Idle;
        }

        public void MarkCleared()
        {
            _aliveCount = 0;
            State = SpawnSystemState.Cleared;
        }

        public bool CanSpawnNow()
        {
            if (State != SpawnSystemState.Active)
                return false;

            return _aliveCount < _maxAliveCount;
        }

        public bool ShouldContinueLoop()
        {
            if (State == SpawnSystemState.Stopped)
                return false;

            if (!_loopSpawn && _hasSpawnedOnce)
                return false;

            return true;
        }

        public void NotifySpawned()
        {
            _aliveCount++;
            _hasSpawnedOnce = true;
        }

        public void NotifyReleased()
        {
            _aliveCount--;
            if (_aliveCount < 0)
                _aliveCount = 0;
        }
    }
}
