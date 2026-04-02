using UnityEngine;
using UnityEngine.UIElements;

namespace ParallelWorld
{
    /// <summary>
    /// 玩家对话 UI：与 <see cref="PromptViewController"/> 相同的屏幕定位规则；锚点为当前激活的 Real/Shadow 躯体；
    /// 世界偏移与竖直视口带使用 <see cref="InteractionConfig.dialogueWorldOffset"/> / <see cref="InteractionConfig.dialogueTopViewport"/>（经 InteractionController 注入）。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DialogueViewController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private Camera _camera;
        [SerializeField, Tooltip("Player 根物体（含 RealPlayer / ShadowPlayer 子级）；留空则运行时查找 MovementController")]
        private Transform _playerRoot;
        [SerializeField, Tooltip("未注入 InteractionConfig 时使用的世界坐标偏移（语义同 Config.dialogueWorldOffset）")]
        private Vector3 _worldOffset = new Vector3(0f, 1f, 0f);
        [SerializeField, Tooltip("提示框中心相对屏幕坐标的偏移（像素），与 PromptViewController 一致")]
        private Vector2 _screenOffset = new Vector2(0f, 20f);
        [SerializeField, Tooltip("未指定 Config 时的备用竖直视口比例 0~1；有 Config 时用 dialogueTopViewport")]
        [Range(0f, 1f)]
        private float _fallbackTopViewport = 0.833f;

        private InteractionConfig _config;
        private VisualElement _root;
        private Label _dialogueLabel;
        private bool _wantsVisible;

        /// <summary>由 InteractionController 注入，用于读取 dialogueWorldOffset / dialogueTopViewport</summary>
        public void SetConfig(InteractionConfig config) => _config = config;

        private void Awake()
        {
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();
            if (_camera == null)
                _camera = Camera.main;

            if (_uiDocument != null)
            {
                _root = _uiDocument.rootVisualElement;
                _dialogueLabel = _root?.Q<Label>("Dialogue");
            }

            if (_dialogueLabel == null)
                Debug.LogWarning("[DialogueViewController] 未找到名为 Dialogue 的 Label，请确认 UIDocument 已加载 Dialogue.uxml");

            ResolvePlayerRootIfNeeded();
            Hide();
        }

        private void ResolvePlayerRootIfNeeded()
        {
            if (_playerRoot != null)
                return;
            var movement = Object.FindAnyObjectByType<MovementController>();
            if (movement != null)
                _playerRoot = movement.transform;
        }

        private Transform ResolveActiveBodyTransform()
        {
            ResolvePlayerRootIfNeeded();
            if (_playerRoot == null)
                return null;

            Transform real = _playerRoot.Find("RealPlayer");
            Transform shadow = _playerRoot.Find("ShadowPlayer");

            if (real != null && real.gameObject.activeInHierarchy)
                return real;
            if (shadow != null && shadow.gameObject.activeInHierarchy)
                return shadow;
            return _playerRoot;
        }

        private void LateUpdate()
        {
            if (!_wantsVisible || _root == null || _camera == null)
                return;

            Transform body = ResolveActiveBodyTransform();
            if (body == null)
                return;

            Vector3 worldOffset = _config != null ? _config.dialogueWorldOffset : _worldOffset;
            float topViewport = _config != null ? _config.dialogueTopViewport : _fallbackTopViewport;
            ProximityPromptLayout.Apply(_root, _camera, body.position, worldOffset, _screenOffset, topViewport);
        }

        /// <summary>显示对话并每帧跟随当前激活的玩家躯体。</summary>
        public void Show(string text)
        {
            SetText(text);
            _wantsVisible = true;
            if (_root != null)
                _root.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            _wantsVisible = false;
            var root = _root ?? _uiDocument?.rootVisualElement;
            if (root != null)
            {
                root.style.display = DisplayStyle.None;
                if (_root == null)
                    _root = root;
            }
        }

        public void SetText(string text)
        {
            if (_dialogueLabel != null)
                _dialogueLabel.text = text ?? "";
        }
    }
}
