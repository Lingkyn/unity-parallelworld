using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 死亡/复活 VFX 占位实现：可分别挂于不同 GameObject，在 Inspector 中分别拖入 DeathRespawnController 的 deathVfx、respawnVfx。
    /// 实现 IDeathVFX 与 IRespawnVFX，方法留空，后续可接入粒子、屏幕效果等。
    /// </summary>
    public class DeathRespawnVFXPlaceholder : MonoBehaviour, IDeathVFX, IRespawnVFX
    {
        public void OnDeath(Vector3 position, Vector3 forward)
        {
            // 占位：接入粒子、屏幕效果等时可在此实现；调试时可取消下行注释
            // Debug.Log($"[DeathRespawnVFX] 死亡特效 @ {position}");
        }

        public void OnRespawn(Vector3 position, Vector3 forward)
        {
            // 占位：接入粒子、屏幕效果等时可在此实现；调试时可取消下行注释
            // Debug.Log($"[DeathRespawnVFX] 复活特效 @ {position}");
        }
    }
}
