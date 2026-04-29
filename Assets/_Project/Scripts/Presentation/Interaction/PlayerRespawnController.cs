using UnityEngine;

namespace ParallelWorld
{
    public class PlayerRespawnController : MonoBehaviour
    {
        [Header("移动控制器")]
        [SerializeField] private MovementController movementController;

        [Header("重生偏移")]
        [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0);

        [Header("音效")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip respawnSound;

        private Vector3 lastRespawnPosition;
        private bool hasRespawnPoint = false;

        private void OnEnable()
        {
            EventBus.RespawnPointActivated += OnRespawnPointActivated;
        }

        private void OnDisable()
        {
            EventBus.RespawnPointActivated -= OnRespawnPointActivated;
        }

        private void OnRespawnPointActivated(Vector3 position, string checkpointId, int order)
        {
            lastRespawnPosition = position;
            hasRespawnPoint = true;

            Debug.Log("记录新的重生点：" + checkpointId + " @ " + position);
        }

        public void RespawnPlayer()
        {
            if (!hasRespawnPoint)
            {
                Debug.LogWarning("还没有经过任何重生点");
                return;
            }

            if (movementController == null)
            {
                Debug.LogError("MovementController 没有绑定");
                return;
            }

            Vector3 targetPos = lastRespawnPosition + offset;

            Debug.Log("点击重生按钮，回到：" + targetPos);

            // ? 真正传送（走你的系统）
            movementController.TeleportTo(targetPos);

            // ? 播放音效
            if (audioSource != null && respawnSound != null)
            {
                audioSource.PlayOneShot(respawnSound);
            }

            EventBus.PublishPlayerRespawned(targetPos);
        }
    }
}