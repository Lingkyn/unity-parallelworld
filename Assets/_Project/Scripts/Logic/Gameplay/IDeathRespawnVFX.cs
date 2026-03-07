using UnityEngine;

namespace ParallelWorld
{
    /// <summary>死亡时在玩家位置播放特效</summary>
    public interface IDeathVFX
    {
        void OnDeath(Vector3 position, Vector3 forward);
    }

    /// <summary>复活时在复活点位置播放特效</summary>
    public interface IRespawnVFX
    {
        void OnRespawn(Vector3 position, Vector3 forward);
    }
}
