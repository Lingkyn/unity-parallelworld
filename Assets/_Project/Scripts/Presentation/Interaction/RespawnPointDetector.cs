using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 挂于复活点物体，OnTriggerEnter 时若碰撞体为 Player 则激活该复活点。
    /// 复活点物体需：Collider(IsTrigger=true)、Tag=Respawn。
    /// 接入检查点/存档系统时可增加 checkpointId、order 字段。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class RespawnPointDetector : MonoBehaviour
    {
        private DeathRespawnController _controller;

        private void Awake()
        {
            _controller = FindFirstObjectByType<DeathRespawnController>();
            if (_controller == null)
                Debug.LogWarning("[RespawnPointDetector] 未找到 DeathRespawnController，确保场景中有 Player");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null || other.gameObject == null) return;
            // 碰撞体在 RealPlayer/ShadowPlayer 上，其 Tag 为 Player
            if (!other.CompareTag(Tags.Player)) return;
            if (_controller == null) return;

            _controller.HandleRespawnPointActivated(transform.position, null);
        }
    }
}
