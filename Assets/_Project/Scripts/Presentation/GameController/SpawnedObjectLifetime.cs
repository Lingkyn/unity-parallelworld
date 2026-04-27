using System;
using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 挂在已生成对象上，负责在生命周期结束或对象被禁用/销毁时回调释放事件。
    /// </summary>
    public class SpawnedObjectLifetime : MonoBehaviour
    {
        private Action<SpawnedObjectLifetime> _onReleased;
        private float _remainingLifetime;
        private bool _initialized;
        private bool _released;
        private bool _timedRelease;

        /// <param name="lifetime">秒；&lt;= 0 表示不按时间自动释放，仅在被 Destroy/Disable 时回调（配合 HazardDespawnAfterPlayerRespawn 等）。</param>
        public void Initialize(float lifetime, Action<SpawnedObjectLifetime> onReleased)
        {
            _timedRelease = lifetime > 0f;
            _remainingLifetime = lifetime;
            _onReleased = onReleased;
            _initialized = true;
            _released = false;
            enabled = true;
        }

        private void Update()
        {
            if (!_initialized || _released || !_timedRelease)
                return;

            _remainingLifetime -= Time.deltaTime;
            if (_remainingLifetime <= 0f)
                Release();
        }

        private void OnDisable()
        {
            if (!_initialized || _released)
                return;

            Release();
        }

        private void Release()
        {
            if (_released)
                return;

            _released = true;
            _onReleased?.Invoke(this);
        }
    }
}
