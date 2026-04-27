using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 重生复位策略（ScriptableObject）：与场景内 <see cref="RespawnResetZone"/> 组合使用，
    /// 决定在收到 <see cref="EventBus.PlayerRespawned"/> 时执行哪些动作。
    /// </summary>
    [CreateAssetMenu(fileName = "RespawnResetProfile", menuName = "ParallelWorld/Respawn Reset Profile")]
    public class RespawnResetProfile : ScriptableObject
    {
        [Tooltip("清理列表中的 ObjectSpawnZone：销毁其记录的全部生成物并回收生成名额")]
        public bool clearAssociatedSpawners = true;

        [Tooltip("销毁 RespawnResetZone 上配置的 extraDestroyRoots")]
        public bool destroyListedRoots = true;
    }
}
