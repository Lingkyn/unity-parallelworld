using System;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace ParallelWorld
{
    /// <summary>
    /// 单实例交互按钮控制器：靠近时显示，点击触发回调
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class InteractButtonViewController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private Camera _camera;
        [SerializeField, Tooltip("按钮相对屏幕坐标的偏移（像素）")]
        private Vector2 _screenOffset = new Vector2(0, 20);
        [SerializeField, Tooltip("未指定 Config 时的备用视口比例 0~1；若 InteractionController 注入 Config 则优先用 Config.promptTopViewport")]
        [Range(0f, 1f)]
        private float _fallbackTopViewport = 0.833f;

        private InteractionConfig _config;

        private Button _button;
        private VisualElement _root;
        private bool _clickProcessed;
        private GameObject _currentTarget;
        private Vector3 _lastWorldOffset;

        /// <summary>
        /// 按钮被点击时触发，参数为当前关联的可交互物（由 InteractionController 设置）
        /// 重复快速点击只处理一次
        /// </summary>
        public event Action<GameObject> OnClicked;

        private void Awake()
        {
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
                if (_uiDocument == null)
                    Debug.LogWarning("[InteractButtonViewController] 请在 Inspector 中指定 UIDocument");
            }
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                    Debug.LogWarning("[InteractButtonViewController] 请在 Inspector 中指定 Camera");
            }

            if (_uiDocument != null)
            {
                _root = _uiDocument.rootVisualElement;
                _button = _root?.Q<Button>("InteractButton") ?? _root?.Q<Button>();
            }

            if (_button != null)
                _button.clicked += OnButtonClicked;

            if (_button == null)
                Debug.LogWarning("[InteractButtonViewController] 未找到名为 'InteractButton' 的 Button，请检查 UIDocument 是否加载 InteractButton.uxml");
        }

        /// <summary>由 InteractionController 注入，用于读取 promptTopViewport</summary>
        public void SetConfig(InteractionConfig config) => _config = config;

        private void LateUpdate()
        {
            if (_currentTarget != null && _root != null && _root.style.display == DisplayStyle.Flex)
                SetPosition(_currentTarget.transform.position, _lastWorldOffset);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.clicked -= OnButtonClicked;
        }

        private void OnButtonClicked()
        {
            if (_clickProcessed || _currentTarget == null) return;
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
                if (_root == null) _root = root;
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
        /// </summary>
        public void SetPosition(Vector3 worldPosition, Vector3 worldOffset)
        {
            if (_camera == null || _root == null) return;

            _lastWorldOffset = worldOffset;
            float topViewport = _config != null ? _config.promptTopViewport : _fallbackTopViewport;
            ProximityPromptLayout.Apply(_root, _camera, worldPosition, worldOffset, _screenOffset, topViewport);
        }
    }
}
