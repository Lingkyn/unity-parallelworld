using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 挑战区重生复位：订阅 <see cref="EventBus.PlayerRespawned"/>，按 <see cref="RespawnResetProfile"/>
    /// 与本组件引用对 ObjectSpawnZone 与额外物体执行清理。仅处理 Inspector 显式引用（范围分区）。
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class RespawnResetZone : MonoBehaviour
    {
        [SerializeField, Tooltip("可选；为空则默认两项均为 true（见 Awake）")]
        private RespawnResetProfile profile;

        [SerializeField]
        private ObjectSpawnZone[] spawnZonesToClear;

        [SerializeField, Tooltip("复活后额外销毁的根物体（如临时机关实例）")]
        private GameObject[] extraDestroyRoots;

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
            bool clearSpawners = profile == null || profile.clearAssociatedSpawners;
            bool destroyExtras = profile == null || profile.destroyListedRoots;

            if (clearSpawners && spawnZonesToClear != null)
            {
                for (int i = 0; i < spawnZonesToClear.Length; i++)
                {
                    var zone = spawnZonesToClear[i];
                    if (zone != null)
                        zone.ClearSpawnedObjects();
                }
            }

            if (destroyExtras && extraDestroyRoots != null)
            {
                for (int i = 0; i < extraDestroyRoots.Length; i++)
                {
                    var go = extraDestroyRoots[i];
                    if (go != null)
                        Destroy(go);
                }
            }
        }
    }
}
