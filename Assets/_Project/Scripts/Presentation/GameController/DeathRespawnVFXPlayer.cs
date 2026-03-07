using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 在指定位置实例化并播放特效预制体，实现 IDeathVFX / IRespawnVFX。
    /// 用法：挂到 Player 下，把 CFXR 等预制体拖到 deathEffectPrefab / respawnEffectPrefab，
    /// 再将本组件拖入 DeathRespawnController 的 deathVfx、respawnVfx 槽位。
    /// </summary>
    public class DeathRespawnVFXPlayer : MonoBehaviour, IDeathVFX, IRespawnVFX
    {
        [SerializeField, Tooltip("死亡时播放的特效预制体（如 CFXR2 Skull Head Alt）")]
        private GameObject deathEffectPrefab;
        [SerializeField, Tooltip("复活时播放的特效预制体（如 CFXR3 Magic Aura A）")]
        private GameObject respawnEffectPrefab;
        [SerializeField, Tooltip("特效实例自动销毁延迟（秒），0 则不主动销毁")]
        private float autoDestroyDelay = 5f;
        [SerializeField, Tooltip("死亡特效相对生成点的世界空间偏移")]
        private Vector3 deathEffectOffset = Vector3.zero;
        [SerializeField, Tooltip("复活特效相对生成点的世界空间偏移")]
        private Vector3 respawnEffectOffset = Vector3.zero;

        public void OnDeath(Vector3 position, Vector3 forward)
        {
            if (deathEffectPrefab != null)
                SpawnEffect(deathEffectPrefab, position + deathEffectOffset, forward);
        }

        public void OnRespawn(Vector3 position, Vector3 forward)
        {
            if (respawnEffectPrefab != null)
                SpawnEffect(respawnEffectPrefab, position + respawnEffectOffset, forward);
        }

        private void SpawnEffect(GameObject prefab, Vector3 position, Vector3 forward)
        {
            var instance = Instantiate(prefab, position, Quaternion.LookRotation(forward));
            if (autoDestroyDelay > 0f)
                Destroy(instance, autoDestroyDelay);
        }
    }
}
