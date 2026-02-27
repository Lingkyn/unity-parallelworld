using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 交互核心逻辑：维护当前目标、计算 UI 位置
    /// </summary>
    public class InteractionCore
    {
        private GameObject _currentInteractable;
        private bool _isPromptVisible;

        public GameObject CurrentInteractable => _currentInteractable;
        public bool IsPromptVisible => _isPromptVisible;

        public void SetCurrent(GameObject go)
        {
            _currentInteractable = go;
        }

        public void ShowPrompt()
        {
            _isPromptVisible = true;
        }

        public void HidePrompt()
        {
            _isPromptVisible = false;
            _currentInteractable = null;
        }

        /// <summary>
        /// 根据世界坐标计算屏幕坐标（用于 UI 定位）
        /// </summary>
        public static Vector2 GetScreenPosition(Camera cam, Vector3 worldPos, Vector3 offset)
        {
            if (cam == null) return Vector2.zero;
            Vector3 target = worldPos + offset;
            return cam.WorldToScreenPoint(target);
        }
    }
}
