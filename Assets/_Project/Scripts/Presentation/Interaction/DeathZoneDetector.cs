using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 挂于死亡区域物体，OnTriggerEnter 时若碰撞体为 Player 则发布 PlayerDeathRequested 事件。
    /// DeathRespawnController 通过 EventBus 订阅并执行死亡流程，实现解耦。
    /// 死亡区域物体需：Collider(IsTrigger=true)、Layer=DeathZone；Physics 矩阵需允许 Player 与 DeathZone 触发。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DeathZoneDetector : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other == null || other.gameObject == null) return;
            if (!other.CompareTag(Tags.Player)) return;

            EventBus.PublishPlayerDeathRequested();
        }
    }
}
