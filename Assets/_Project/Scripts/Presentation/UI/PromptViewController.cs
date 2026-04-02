using UnityEngine;
using UnityEngine.UIElements;

namespace ParallelWorld
{
    /// <summary>
    /// 单实例 Floating Prompt 控制器
    /// 使用 UIDocument 加载 Prompt.uxml，根据世界坐标定位 Label
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class PromptViewController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private Camera _camera;
        [SerializeField, Tooltip("提示框中心相对屏幕坐标的偏移（像素）")]
        private Vector2 _screenOffset = new Vector2(0, 20);
        [SerializeField, Tooltip("未指定 Config 时的备用视口比例 0~1；若 InteractionController 注入 Config 则优先用 Config.promptTopViewport")]
        [Range(0f, 1f)]
        private float _fallbackTopViewport = 0.833f;

        private InteractionConfig _config;
        private GameObject _currentTarget;
        private Vector3 _lastWorldOffset;
        private Label _promptLabel;
        private VisualElement _root;

        private void Awake()
        {
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
                if (_uiDocument == null)
                    Debug.LogWarning("[PromptViewController] 请在 Inspector 中指定 UIDocument");
            }
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                    Debug.LogWarning("[PromptViewController] 请在 Inspector 中指定 Camera");
            }

            if (_uiDocument != null)
            {
                _root = _uiDocument.rootVisualElement;
                _promptLabel = _root?.Q<Label>("Prompt");
            }

            if (_promptLabel == null)
                Debug.LogWarning("[PromptViewController] 未找到名为 'Prompt' 的 Label，请检查 UIDocument 是否加载 Prompt.uxml");
        }

        /// <summary>由 InteractionController 注入，用于读取 promptTopViewport</summary>
        public void SetConfig(InteractionConfig config) => _config = config;

        private void LateUpdate()
        {
            if (_currentTarget != null && _root != null && _root.style.display == DisplayStyle.Flex)
                SetPosition(_currentTarget.transform.position, _lastWorldOffset);
        }

        /// <summary>
        /// 显示提示，并关联可交互物用于每帧跟随
        /// </summary>
        /// <param name="target">可交互物，传入 null 时仅显示不跟随</param>
        public void Show(GameObject target = null)
        {
            _currentTarget = target;
            if (_root != null)
                _root.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// 隐藏提示
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
        /// 设置提示文本
        /// </summary>
        public void SetText(string text)
        {
            if (_promptLabel != null)
                _promptLabel.text = text ?? "";
        }

        /// <summary>
        /// 根据世界坐标设置 UI 位置（World-to-Screen）
        /// </summary>
        public void SetPosition(Vector3 worldPosition, Vector3 worldOffset)
        {
            if (_camera == null || _root == null) return;

            _lastWorldOffset = worldOffset;
            float topViewport = _config != null ? _config.promptTopViewport : _fallbackTopViewport;
            ProximityPromptLayout.Apply(_root, _camera, worldPosition, worldOffset, _screenOffset, topViewport);
        }
    }

    /// <summary>
    /// 靠近提示、玩家对话、交互按钮共用的屏幕定位：水平跟随世界锚点投影，竖直由调用方传入的 <paramref name="topViewport"/> 决定。
    /// </summary>
    public static class ProximityPromptLayout
    {
        public static void Apply(
            VisualElement root,
            Camera camera,
            Vector3 worldPosition,
            Vector3 worldOffset,
            Vector2 screenOffset,
            float topViewport)
        {
            if (camera == null || root == null)
                return;

            Vector3 anchor = worldPosition + worldOffset;
            Vector2 screenPos = camera.WorldToScreenPoint(anchor);
            screenPos += screenOffset;

            float viewportX = Screen.width > 0 ? screenPos.x / Screen.width : 0f;
            root.style.left = Length.Percent(viewportX * 100f);
            root.style.translate = new Translate(Length.Percent(-50), Length.Percent(0));
            root.style.top = Length.Percent(topViewport * 100f);
            root.style.right = StyleKeyword.Auto;
            root.style.bottom = StyleKeyword.Auto;
            root.style.position = Position.Absolute;
        }
    }
}
