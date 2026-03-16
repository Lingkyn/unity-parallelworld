using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 挂于死亡区域物体，OnTriggerEnter 时若碰撞体为 Player 则通知 DeathRespawnController。
    /// 死亡区域物体需：Collider(IsTrigger=true)、Layer=DeathZone；Physics 矩阵需允许 Player 与 DeathZone 触发。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DeathZoneDetector : MonoBehaviour
    {
        private DeathRespawnController _controller;

        private void Awake()
        {
            _controller = ServiceLocator.Get<DeathRespawnController>();
            if (_controller == null)
                Debug.LogWarning("[DeathZoneDetector] 未找到 DeathRespawnController，确保场景中有 Player");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null || other.gameObject == null) return;
            if (!other.CompareTag(Tags.Player)) return;
            if (_controller == null)
                _controller = ServiceLocator.Get<DeathRespawnController>();
            if (_controller == null) return;

            _controller.HandleDeathRequested();
        }
    }
}
