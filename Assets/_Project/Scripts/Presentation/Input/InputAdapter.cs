using UnityEngine;
using UnityEngine.InputSystem;

namespace ParallelWorld
{
    /// <summary>
    /// 输入意图适配器：将原始输入转换为移动/跳跃意图
    /// 支持手动指定 InputActionAsset，或从同物体上的 PlayerInput 组件自动获取
    /// </summary>
    public class InputAdapter : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _togglePlayerAction;

        private void Awake()
        {
            if (inputActions == null)
            {
                var playerInput = GetComponent<PlayerInput>();
                if (playerInput != null)
                    inputActions = playerInput.actions;
            }

            if (inputActions == null)
            {
                Debug.LogWarning("[InputAdapter] 未找到 InputActionAsset，请手动指定或确保同物体有 PlayerInput 组件");
                return;
            }

            var playerMap = inputActions.FindActionMap("Player");
            _moveAction = playerMap?.FindAction("Move");
            _jumpAction = playerMap?.FindAction("Jump");
            _togglePlayerAction = playerMap?.FindAction("TogglePlayer");

            if (_moveAction == null || _jumpAction == null)
                Debug.LogWarning("[InputAdapter] 未找到 Player 动作映射中的 Move/Jump");

            _moveAction?.Enable();
            _jumpAction?.Enable();
            _togglePlayerAction?.Enable();
        }

        private void OnDestroy()
        {
            _moveAction?.Disable();
            _jumpAction?.Disable();
            _togglePlayerAction?.Disable();
        }

        /// <summary>
        /// 获取移动意图 (归一化方向，世界空间 XZ 平面)
        /// </summary>
        public Vector2 GetMoveIntent()
        {
            if (_moveAction == null) return Vector2.zero;
            return _moveAction.ReadValue<Vector2>();
        }

        /// <summary>
        /// 本帧是否按下跳跃
        /// </summary>
        public bool GetJumpPressed()
        {
            if (_jumpAction == null) return false;
            return _jumpAction.WasPressedThisFrame();
        }

        /// <summary>
        /// 本帧是否按下切换躯体键（X）
        /// </summary>
        public bool GetTogglePlayerPressed()
        {
            if (_togglePlayerAction == null) return false;
            return _togglePlayerAction.WasPressedThisFrame();
        }
    }
}
