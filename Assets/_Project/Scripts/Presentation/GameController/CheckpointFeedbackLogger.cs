using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 到达检查点时的简单反馈：输出日志。可替换为 UI 文案或音效组件（实现 ICheckpointFeedback）。
    /// 预留音效/特效槽位，后续可接入播放。
    /// </summary>
    public class CheckpointFeedbackLogger : MonoBehaviour, ICheckpointFeedback
    {
        [Tooltip("是否在 Console 输出")]
        [SerializeField] private bool logToConsole = true;

        [Header("音效与特效")]
        [SerializeField, Tooltip("到达检查点时播放（需本物体或子物体有 AudioSource）")]
        private AudioClip optionalSoundEffect;
        [SerializeField, Tooltip("在检查点位置实例化播放")]
        private GameObject optionalVfxPrefab;
        [SerializeField, Tooltip("特效实例多少秒后销毁，0 表示不自动销毁")]
        private float vfxDestroyAfterSeconds = 5f;

        public void ShowCheckpointReached()
        {
            if (logToConsole)
                Debug.Log("[Checkpoint] 检查点已记录");
            // 预留：后续可在此播放 optionalSoundEffect
        }

        public void ShowCheckpointReached(Vector3 worldPosition)
        {
            if (logToConsole)
                Debug.Log($"[Checkpoint] 检查点已记录 @ {worldPosition}");

            if (optionalSoundEffect != null)
            {
                var audioSource = GetComponentInChildren<AudioSource>();
                if (audioSource != null)
                    audioSource.PlayOneShot(optionalSoundEffect);
            }

            if (optionalVfxPrefab != null)
            {
                var instance = Object.Instantiate(optionalVfxPrefab, worldPosition, Quaternion.identity);
                if (vfxDestroyAfterSeconds > 0f)
                    Object.Destroy(instance, vfxDestroyAfterSeconds);
            }
        }
    }
}
