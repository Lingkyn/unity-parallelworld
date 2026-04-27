using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 危险物在玩家完成一次死亡→复活流程后再销毁自身。
    /// 仅当本物体带 <see cref="SpawnedObjectLifetime"/>（由 <see cref="ObjectSpawnZone"/> 生成时添加）时才销毁，
    /// 避免场景里手动摆的静态老鼠在每次复活时被清掉。
    /// </summary>
    public class HazardDespawnAfterPlayerRespawn : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.PlayerRespawned += OnPlayerRespawned;
        }

        private void OnDisable()
        {
            EventBus.PlayerRespawned -= OnPlayerRespawned;
        }

        private void OnPlayerRespawned(Vector3 _)
        {
            if (GetComponent<SpawnedObjectLifetime>() == null)
                return;
            Destroy(gameObject);
        }
    }
}
