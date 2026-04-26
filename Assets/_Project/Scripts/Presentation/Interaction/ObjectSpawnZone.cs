using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParallelWorld
{
    public enum SpawnPointMode
    {
        First,
        Random,
        RoundRobin,
        RandomNoRepeat
    }

    /// <summary>
    /// 生成区触发器：玩家进入后按配置驱动生成，支持 Pause/Resume/Stop/Clear。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ObjectSpawnZone : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private ObjectSpawnerConfig config;
        [SerializeField] private Transform[] spawnPoints;
        [Header("点位策略")]
        [SerializeField] private SpawnPointMode spawnPointMode = SpawnPointMode.Random;

        private readonly HashSet<GameObject> _spawnedObjects = new HashSet<GameObject>();
        private readonly List<Transform> _validSpawnPoints = new List<Transform>();
        private ObjectSpawnSystem _system;
        private Coroutine _spawnCoroutine;
        private bool _isPlayerInside;
        private int _roundRobinIndex;
        private int _lastRandomIndex = -1;

        private void Awake()
        {
            _system = new ObjectSpawnSystem();
            if (config != null)
                _system.Configure(config.rule);

            var zoneCollider = GetComponent<Collider>();
            if (zoneCollider != null && !zoneCollider.isTrigger)
                Debug.LogWarning("[ObjectSpawnZone] Collider 应设置为 Is Trigger。");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsTriggerTarget(other))
                return;

            _isPlayerInside = true;
            _system.Activate();

            if (config != null && config.rule != null && config.rule.startOnEnter)
                StartSpawnLoopIfNeeded();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsTriggerTarget(other))
                return;

            _isPlayerInside = false;
            if (config != null && config.rule != null && config.rule.stopWhenPlayerExit)
                DeactivateOnExit();
        }

        public void Pause()
        {
            _system.Pause();
        }

        public void Resume()
        {
            _system.Resume();
            StartSpawnLoopIfNeeded();
        }

        public void Stop()
        {
            _system.Stop();
            StopSpawnLoop();
        }

        public void StartSpawning()
        {
            _system.Activate();
            StartSpawnLoopIfNeeded();
        }

        public void ClearSpawnedObjects()
        {
            var snapshot = new List<GameObject>(_spawnedObjects);
            for (int i = 0; i < snapshot.Count; i++)
            {
                var spawned = snapshot[i];
                if (spawned != null)
                    Destroy(spawned);
            }

            _spawnedObjects.Clear();
            _system.MarkCleared();
        }

        private bool IsTriggerTarget(Collider other)
        {
            if (other == null || other.gameObject == null || config == null || config.rule == null)
                return false;

            string targetTag = string.IsNullOrEmpty(config.rule.triggerTag) ? Tags.Player : config.rule.triggerTag;
            return other.CompareTag(targetTag);
        }

        private void StartSpawnLoopIfNeeded()
        {
            if (_spawnCoroutine != null)
                return;

            if (_system.State == SpawnSystemState.Stopped)
                return;

            _spawnCoroutine = StartCoroutine(SpawnLoopCoroutine());
        }

        private void StopSpawnLoop()
        {
            if (_spawnCoroutine == null)
                return;

            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        private IEnumerator SpawnLoopCoroutine()
        {
            while (_system.ShouldContinueLoop())
            {
                if (_isPlayerInside && _system.CanSpawnNow())
                    SpawnOne();

                float interval = GetSpawnInterval();
                yield return new WaitForSeconds(interval);
            }

            _spawnCoroutine = null;
        }

        private void SpawnOne()
        {
            if (config == null)
                return;

            SpawnEntry entry = config.GetPrimaryEntry();
            if (entry == null || entry.prefab == null)
            {
                Debug.LogWarning("[ObjectSpawnZone] 缺少可用 SpawnEntry 或 Prefab。");
                return;
            }

            Transform spawnPoint = GetSpawnPoint();
            GameObject spawned = Instantiate(entry.prefab, spawnPoint.position, spawnPoint.rotation);
            _spawnedObjects.Add(spawned);
            _system.NotifySpawned();

            ApplyInitialVelocity(spawned, entry);

            var lifetime = spawned.GetComponent<SpawnedObjectLifetime>();
            if (lifetime == null)
                lifetime = spawned.AddComponent<SpawnedObjectLifetime>();
            lifetime.Initialize(entry.lifetime, OnSpawnedObjectReleased);
        }

        private Transform GetSpawnPoint()
        {
            CacheValidSpawnPoints();
            if (_validSpawnPoints.Count == 0)
                return transform;

            switch (spawnPointMode)
            {
                case SpawnPointMode.RoundRobin:
                    {
                        int index = _roundRobinIndex % _validSpawnPoints.Count;
                        _roundRobinIndex = (_roundRobinIndex + 1) % _validSpawnPoints.Count;
                        return _validSpawnPoints[index];
                    }
                case SpawnPointMode.RandomNoRepeat:
                    {
                        if (_validSpawnPoints.Count == 1)
                            return _validSpawnPoints[0];

                        int randomIndex = Random.Range(0, _validSpawnPoints.Count);
                        if (randomIndex == _lastRandomIndex)
                            randomIndex = (randomIndex + 1) % _validSpawnPoints.Count;
                        _lastRandomIndex = randomIndex;
                        return _validSpawnPoints[randomIndex];
                    }
                case SpawnPointMode.Random:
                    {
                        int randomIndex = Random.Range(0, _validSpawnPoints.Count);
                        _lastRandomIndex = randomIndex;
                        return _validSpawnPoints[randomIndex];
                    }
                case SpawnPointMode.First:
                default:
                    return _validSpawnPoints[0];
            }
        }

        private void ApplyInitialVelocity(GameObject spawned, SpawnEntry entry)
        {
            if (spawned == null || entry == null || entry.initialSpeed <= 0f)
                return;

            var rb = spawned.GetComponent<Rigidbody>();
            if (rb == null)
                return;

            Vector3 direction = entry.initialDirection.sqrMagnitude <= 0.0001f
                ? transform.forward
                : entry.initialDirection.normalized;
            rb.linearVelocity = direction * entry.initialSpeed;
        }

        private void CacheValidSpawnPoints()
        {
            _validSpawnPoints.Clear();
            if (spawnPoints == null || spawnPoints.Length == 0)
                return;

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                    _validSpawnPoints.Add(spawnPoints[i]);
            }
        }

        private void DeactivateOnExit()
        {
            _system.Deactivate();
            StopSpawnLoop();
        }

        private void OnSpawnedObjectReleased(SpawnedObjectLifetime lifetime)
        {
            if (lifetime == null)
                return;

            GameObject target = lifetime.gameObject;
            _spawnedObjects.Remove(target);
            _system.NotifyReleased();

            if (target != null && target.activeInHierarchy)
                Destroy(target);
        }

        private float GetSpawnInterval()
        {
            if (config == null || config.rule == null || config.rule.spawnInterval <= 0f)
                return 1f;

            return config.rule.spawnInterval;
        }
    }
}
