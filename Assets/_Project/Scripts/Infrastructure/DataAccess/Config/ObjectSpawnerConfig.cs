using System;
using UnityEngine;

namespace ParallelWorld
{
    [Serializable]
    public class SpawnRule
    {
        [Min(0.01f)]
        public float spawnInterval = 1f;
        [Min(1)]
        public int maxAliveCount = 5;
        public bool loopSpawn = true;
        public bool startOnEnter = true;
        public bool stopWhenPlayerExit = false;
        public string triggerTag = Tags.Player;
    }

    [Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        [Min(0.01f)]
        public float lifetime = 5f;
        public Vector3 initialDirection = Vector3.forward;
        [Min(0f)]
        public float initialSpeed = 0f;
    }

    [CreateAssetMenu(fileName = "ObjectSpawnerConfig", menuName = "ParallelWorld/Object Spawner Config")]
    public class ObjectSpawnerConfig : ScriptableObject
    {
        [Header("规则")]
        public SpawnRule rule = new SpawnRule();

        [Header("生成条目")]
        public SpawnEntry[] entries = { new SpawnEntry() };

        public SpawnEntry GetPrimaryEntry()
        {
            if (entries == null || entries.Length == 0)
                return null;

            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i] != null && entries[i].prefab != null)
                    return entries[i];
            }

            return null;
        }
    }
}
