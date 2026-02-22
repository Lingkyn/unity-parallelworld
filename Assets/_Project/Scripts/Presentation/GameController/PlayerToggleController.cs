using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 玩家躯体切换控制器：按 X 键在 RealPlayer 与 ShadowPlayer 之间切换显示
    /// 与光圈系统同时响应 X 键，各自独立处理
    /// </summary>
    public class PlayerToggleController : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField, Tooltip("实体玩家（Real 躯体）")]
        private GameObject realPlayer;
        [SerializeField, Tooltip("影子玩家（Shadow 躯体）")]
        private GameObject shadowPlayer;
        [SerializeField] private InputAdapter inputAdapter;
        [SerializeField, Tooltip("默认显示的躯体")]
        private PlayerForm defaultForm = PlayerForm.Real;

        private PlayerForm _currentForm;

        private void Awake()
        {
            if (inputAdapter == null)
                inputAdapter = GetComponent<InputAdapter>();

            if (realPlayer == null || shadowPlayer == null)
            {
                // 尝试从子物体自动查找
                foreach (Transform child in transform)
                {
                    if (child.name.Equals("RealPlayer", System.StringComparison.OrdinalIgnoreCase))
                        realPlayer = child.gameObject;
                    else if (child.name.Equals("ShadowPlayer", System.StringComparison.OrdinalIgnoreCase))
                        shadowPlayer = child.gameObject;
                }
            }

            if (realPlayer == null)
                Debug.LogWarning("[PlayerToggleController] 未找到 RealPlayer");
            if (shadowPlayer == null)
                Debug.LogWarning("[PlayerToggleController] 未找到 ShadowPlayer");
            if (inputAdapter == null)
                Debug.LogWarning("[PlayerToggleController] 未找到 InputAdapter");

            _currentForm = defaultForm;
            ApplyForm();
        }

        private void Update()
        {
            if (inputAdapter == null || !inputAdapter.GetTogglePlayerPressed())
                return;

            Toggle();
        }

        /// <summary>
        /// 切换当前显示的躯体
        /// </summary>
        public void Toggle()
        {
            _currentForm = _currentForm == PlayerForm.Real ? PlayerForm.Shadow : PlayerForm.Real;
            ApplyForm();
        }

        /// <summary>
        /// 设置当前显示的躯体
        /// </summary>
        public void SetActiveForm(PlayerForm form)
        {
            _currentForm = form;
            ApplyForm();
        }

        /// <summary>
        /// 获取当前躯体形态
        /// </summary>
        public PlayerForm CurrentForm => _currentForm;

        private void ApplyForm()
        {
            GameObject toActivate = _currentForm == PlayerForm.Real ? realPlayer : shadowPlayer;
            GameObject toDeactivate = _currentForm == PlayerForm.Real ? shadowPlayer : realPlayer;

            if (toDeactivate != null && toDeactivate.activeSelf && toActivate != null)
            {
                Vector3 syncPos = toDeactivate.transform.position;
                toActivate.transform.position = new Vector3(syncPos.x, syncPos.y, toActivate.transform.position.z);
            }

            if (realPlayer != null)
                realPlayer.SetActive(_currentForm == PlayerForm.Real);
            if (shadowPlayer != null)
                shadowPlayer.SetActive(_currentForm == PlayerForm.Shadow);
        }
    }

    /// <summary>
    /// 玩家躯体形态
    /// </summary>
    public enum PlayerForm
    {
        Real,
        Shadow
    }
}
