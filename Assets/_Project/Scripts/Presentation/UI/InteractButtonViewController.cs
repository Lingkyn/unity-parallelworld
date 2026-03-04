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
        [SerializeField, Range(0.05f, 1f), Tooltip("位置平滑系数，越小越平滑、延迟越大")]
        private float _smoothFactor = 0.25f;

        private Button _button;
        private VisualElement _root;
        private bool _clickProcessed;
        private GameObject _currentTarget;
        private Vector3 _lastWorldOffset;
        private float _smoothedLeft;
        private float _smoothedTop;
        private bool _hasSmoothedPos;

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
            _hasSmoothedPos = false; // 重新显示时重置，避免从旧位置平滑
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
            Vector3 target = worldPosition + worldOffset;
            Vector2 screenPos = _camera.WorldToScreenPoint(target);
            screenPos += _screenOffset;
            float targetLeft = Mathf.Round(screenPos.x);
            float targetTop = Mathf.Round(Screen.height - screenPos.y);

            if (!_hasSmoothedPos)
            {
                _smoothedLeft = targetLeft;
                _smoothedTop = targetTop;
                _hasSmoothedPos = true;
            }
            else
            {
                _smoothedLeft = Mathf.Lerp(_smoothedLeft, targetLeft, _smoothFactor);
                _smoothedTop = Mathf.Lerp(_smoothedTop, targetTop, _smoothFactor);
            }
            _root.style.left = _smoothedLeft;
            _root.style.top = _smoothedTop;
            _root.style.right = StyleKeyword.Auto;
            _root.style.bottom = StyleKeyword.Auto;
            _root.style.position = Position.Absolute;
        }
    }
}
