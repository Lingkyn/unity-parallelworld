using UnityEngine;
using UnityEngine.InputSystem;

namespace ParallelWorld
{
    /// <summary>
    /// 光圈输入适配器：读取 Light 动作映射（鼠标位置、滚轮、切换键）
    /// </summary>
    public class LightInputAdapter : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;

        private InputAction _mousePositionAction;
        private InputAction _mouseScrollAction;
        private InputAction _toggleLightAction;

        private void Awake()
        {
            if (inputActions == null)
            {
                var playerInput = FindAnyObjectByType<UnityEngine.InputSystem.PlayerInput>();
                if (playerInput != null)
                    inputActions = playerInput.actions;
            }

            if (inputActions == null)
            {
                Debug.LogWarning("[LightInputAdapter] 未找到 InputActionAsset，请手动指定或确保场景中有 PlayerInput");
                return;
            }

            var lightMap = inputActions.FindActionMap("Light");
            if (lightMap == null)
            {
                Debug.LogWarning("[LightInputAdapter] 未找到 Light 动作映射");
                return;
            }

            _mousePositionAction = lightMap.FindAction("MousePosition");
            _mouseScrollAction = lightMap.FindAction("MouseScroll");
            _toggleLightAction = lightMap.FindAction("ToggleLight");

            if (_mousePositionAction == null || _mouseScrollAction == null || _toggleLightAction == null)
                Debug.LogWarning("[LightInputAdapter] 未找到 MousePosition/MouseScroll/ToggleLight");

            lightMap.Enable();
        }

        private void OnDestroy()
        {
            inputActions?.FindActionMap("Light")?.Disable();
        }

        /// <summary>
        /// 获取鼠标屏幕坐标
        /// </summary>
        public Vector2 GetMousePosition()
        {
            if (_mousePositionAction == null) return Vector2.zero;
            return _mousePositionAction.ReadValue<Vector2>();
        }

        /// <summary>
        /// 获取鼠标滚轮增量（Y 分量，向上为正）
        /// </summary>
        public float GetScrollDelta()
        {
            if (_mouseScrollAction == null) return 0f;
            return _mouseScrollAction.ReadValue<Vector2>().y;
        }

        /// <summary>
        /// 本帧是否按下切换键
        /// </summary>
        public bool GetTogglePressed()
        {
            if (_toggleLightAction == null) return false;
            return _toggleLightAction.WasPressedThisFrame();
        }
    }
}