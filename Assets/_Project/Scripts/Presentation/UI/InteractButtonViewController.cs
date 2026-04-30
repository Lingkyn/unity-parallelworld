using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ParallelWorld
{
    /// <summary>
    /// 单实例交互按钮控制器：靠近时显示，点击触发回调
    /// 改为与 PromptViewController 相同的逻辑：
    /// UI 完整跟随世界坐标（X、Y 都跟随目标物体）
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class InteractButtonViewController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private Camera _camera;

        [SerializeField, Tooltip("按钮中心相对屏幕坐标的偏移（像素）")]
        private Vector2 _screenOffset = new Vector2(0, -40);

        /// <summary>
        /// 保留 config，主要是为了兼容 InteractionController 中的 SetConfig()
        /// 当前版本不再使用 promptTopViewport，但仍保留接口避免报错
        /// </summary>
        private InteractionConfig _config;

        private Button _button;
        private VisualElement _root;

        private bool _clickProcessed;
        private GameObject _currentTarget;
        private Vector3 _lastWorldOffset;

        /// <summary>
        /// 按钮被点击时触发
        /// 参数为当前关联的可交互物（由 InteractionController 设置）
        /// 重复快速点击只处理一次
        /// </summary>
        public event Action<GameObject> OnClicked;

        private void Awake()
        {
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();

                if (_uiDocument == null)
                    Debug.LogWarning(
                        "[InteractButtonViewController] 请在 Inspector 中指定 UIDocument");
            }

            if (_camera == null)
            {
                _camera = Camera.main;

                if (_camera == null)
                    Debug.LogWarning(
                        "[InteractButtonViewController] 请在 Inspector 中指定 Camera");
            }

            if (_uiDocument != null)
            {
                _root = _uiDocument.rootVisualElement;

                _button =
                    _root?.Q<Button>("InteractButton")
                    ?? _root?.Q<Button>();
            }

            if (_button != null)
                _button.clicked += OnButtonClicked;

            if (_button == null)
                Debug.LogWarning(
                    "[InteractButtonViewController] 未找到名为 'InteractButton' 的 Button，请检查 UIDocument 是否加载 InteractButton.uxml");
        }

        /// <summary>
        /// 保留该方法，兼容 InteractionController 调用
        /// 即使当前逻辑不使用 config，也必须保留
        /// </summary>
        public void SetConfig(InteractionConfig config)
        {
            _config = config;
        }

        private void LateUpdate()
        {
            if (_currentTarget != null &&
                _root != null &&
                _root.style.display == DisplayStyle.Flex)
            {
                SetPosition(
                    _currentTarget.transform.position,
                    _lastWorldOffset);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.clicked -= OnButtonClicked;
        }

        private void OnButtonClicked()
        {
            if (_clickProcessed || _currentTarget == null)
                return;

            _clickProcessed = true;

            OnClicked?.Invoke(_currentTarget);
        }

        /// <summary>
        /// 显示按钮，并关联当前可交互物
        /// </summary>
        public void Show(GameObject target)
        {
            _currentTarget = target;
            _clickProcessed = false;

            if (_root != null)
                _root.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// 隐藏按钮
        /// </summary>
        public void Hide()
        {
            _currentTarget = null;

            var root = _root ?? _uiDocument?.rootVisualElement;

            if (root != null)
            {
                root.style.display = DisplayStyle.None;

                if (_root == null)
                    _root = root;
            }
        }

        /// <summary>
        /// 设置按钮文本
        /// </summary>
        public void SetButtonText(string text)
        {
            if (_button != null)
                _button.text = text ?? "互动";
        }

        /// <summary>
        /// 根据世界坐标设置 UI 位置
        /// 与 PromptViewController 完全一致：
        /// 世界坐标 → 屏幕坐标 → UI Toolkit 绝对定位
        /// </summary>
        public void SetPosition(
            Vector3 worldPosition,
            Vector3 worldOffset)
        {
            if (_camera == null || _root == null)
                return;

            _lastWorldOffset = worldOffset;

            ApplyObjectAnchoredLayout(
                worldPosition,
                worldOffset,
                _screenOffset);
        }

        /// <summary>
        /// 将 UI 完整锚定到物体世界坐标
        /// X、Y 都跟随目标物体移动
        /// </summary>
        private void ApplyObjectAnchoredLayout(
            Vector3 worldPosition,
            Vector3 worldOffset,
            Vector2 screenOffset)
        {
            if (_camera == null || _root == null)
                return;

            Vector3 anchor = worldPosition + worldOffset;

            Vector3 screenPos =
                _camera.WorldToScreenPoint(anchor);

            // 如果目标在摄像机后面，不显示
            if (screenPos.z <= 0f)
                return;

            // UI Toolkit 坐标系：
            // 左上角为原点，Y 轴向下
            float leftPx =
                screenPos.x + screenOffset.x;

            float topPx =
                (Screen.height - screenPos.y)
                + screenOffset.y;

            _root.style.left = leftPx;
            _root.style.top = topPx;

            // 水平居中（让按钮中心对准目标）
            _root.style.translate =
                new Translate(
                    Length.Percent(-50),
                    Length.Percent(0));

            _root.style.right = StyleKeyword.Auto;
            _root.style.bottom = StyleKeyword.Auto;
            _root.style.position = Position.Absolute;
        }
    }
}