using System;
using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 挂于玩家或玩家子物体；监听 OnTriggerEnter/Exit，CompareTag 过滤后触发回调
    /// 需挂载在带 Collider（IsTrigger=true）的物体上
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerDetector : MonoBehaviour
    {
        /// <summary>
        /// 进入可交互物范围时触发，参数为 other（可交互物）
        /// </summary>
        public event Action<GameObject> OnInteractableEnter;

        /// <summary>
        /// 离开可交互物范围时触发
        /// </summary>
        public event Action<GameObject> OnInteractableExit;

        [SerializeField, Tooltip("可交互物 Tag")]
        private string interactableTag = "Interactable";
        [SerializeField, Tooltip("调试：打印所有触发的碰撞（含非 Interactable）")]
        private bool _debugLog;

        private void OnTriggerEnter(Collider other)
        {
            if (other == null || other.gameObject == null) return;

            if (_debugLog)
                Debug.Log($"[TriggerDetector] Enter: {other.gameObject.name}, tag={other.tag}, expect={interactableTag}");

            if (!string.IsNullOrEmpty(interactableTag) && !other.CompareTag(interactableTag)) return;

            OnInteractableEnter?.Invoke(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == null || other.gameObject == null) return;

            if (_debugLog)
                Debug.Log($"[TriggerDetector] Exit: {other.gameObject.name}, tag={other.tag}");

            if (!string.IsNullOrEmpty(interactableTag) && !other.CompareTag(interactableTag)) return;

            OnInteractableExit?.Invoke(other.gameObject);
        }

        /// <summary>
        /// 设置检测的 Tag（供 InteractionController 从 Config 同步）
        /// </summary>
        public void SetInteractableTag(string tag)
        {
            interactableTag = tag ?? "";
        }
    }
}
