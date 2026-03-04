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

        [SerializeField, Tooltip("可交互物 Layer（0 表示不按层筛选）")]
        private LayerMask interactableLayer;
        [SerializeField, Tooltip("调试：打印所有触发的碰撞")]
        private bool _debugLog;

        private bool IsInteractable(GameObject go)
        {
            if (go == null) return false;
            if (interactableLayer != 0 && ((1 << go.layer) & interactableLayer) == 0)
                return false;
            return go.CompareTag(Tags.InteractableButton) || go.CompareTag(Tags.InteractableText);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null || other.gameObject == null) return;

            var go = other.gameObject;
            var pass = IsInteractable(go);
            if (_debugLog)
                Debug.Log($"[TriggerDetector] Enter: {go.name}, layer={go.layer}, tag={go.tag}");

            if (!pass) return;

            OnInteractableEnter?.Invoke(go);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == null || other.gameObject == null) return;

            if (_debugLog)
                Debug.Log($"[TriggerDetector] Exit: {other.gameObject.name}, tag={other.tag}");

            if (!IsInteractable(other.gameObject)) return;

            OnInteractableExit?.Invoke(other.gameObject);
        }

        /// <summary>
        /// 设置检测参数（供 InteractionController 从 Config 同步）
        /// </summary>
        public void SetInteractableFilter(LayerMask layer)
        {
            interactableLayer = layer;
        }
    }
}
