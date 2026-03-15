using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 挂于复活点物体，OnTriggerEnter 时若碰撞体为 Player 则激活该复活点。
    /// 复活点物体需：Collider(IsTrigger=true)、Tag=Respawn。
    /// 检查点 ID 优先从表（CheckpointDatabase + tableIndex）取；无表时用 checkpointId 后备，留空则仅作复活点、不参与存档。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class RespawnPointDetector : MonoBehaviour
    {
        [Tooltip("检查点表，有则从表取 checkpointId；无则用下方后备字段")]
        [SerializeField] private CheckpointDatabase checkpointDatabase;
        [Tooltip("表内条目索引，与 checkpointDatabase 配合使用")]
        [SerializeField] private int tableIndex;
        [Tooltip("无表时手填检查点 ID，留空则仅作复活点、不参与存档")]
        [SerializeField] private string checkpointId;
        [Tooltip("同场景内排序，升序最小者可作为默认出生点")]
        [SerializeField] private int order;

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
            if (!other.CompareTag(Tags.Player)) return;
            if (_controller == null) return;

            string id = ResolveCheckpointId();
            int order = GetOrderForDefaultSpawn();
            _controller.HandleRespawnPointActivated(transform.position, id, order);
        }

        private string ResolveCheckpointId()
        {
            if (checkpointDatabase != null && tableIndex >= 0 && tableIndex < checkpointDatabase.EntryCount)
                return checkpointDatabase.GetCheckpointId(tableIndex);
            return string.IsNullOrEmpty(checkpointId) ? null : checkpointId;
        }

        /// <summary>供默认出生点选取：表驱动时取表条目 order，否则取组件 order。</summary>
        public int GetOrderForDefaultSpawn()
        {
            if (checkpointDatabase != null && tableIndex >= 0 && tableIndex < checkpointDatabase.EntryCount)
            {
                var entry = checkpointDatabase.GetEntry(tableIndex);
                if (entry != null)
                    return entry.order;
            }
            return order;
        }
    }
}
