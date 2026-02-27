using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 交互系统控制器：整合 TriggerDetector、InteractableCache、PromptViewController
    /// 挂于 GameplaySetup 或与 Player 同层
    /// </summary>
    public class InteractionController : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private TriggerDetector _triggerDetector;
        [SerializeField] private PromptViewController _promptViewController;
        [SerializeField] private InteractionConfig _config;
        [SerializeField] private Camera _camera;
        [SerializeField, Tooltip("调试日志")]
        private bool _debugLog;

        private InteractionCore _core;
        private InteractableCache _cache;

        private void Awake()
        {
            _core = new InteractionCore();
            _cache = new InteractableCache();

            if (_camera == null) _camera = Camera.main;
            if (_triggerDetector == null) _triggerDetector = FindAnyObjectByType<TriggerDetector>();
            if (_promptViewController == null) _promptViewController = FindAnyObjectByType<PromptViewController>();

            if (_config != null && _triggerDetector != null)
                _triggerDetector.SetInteractableTag(_config.interactableTag);
        }

        private void Start()
        {
            string tag = _config != null ? _config.interactableTag : "Interactable";
            _cache.Build(tag);

            if (_triggerDetector != null)
            {
                _triggerDetector.OnInteractableEnter += OnInteractableEnter;
                _triggerDetector.OnInteractableExit += OnInteractableExit;
            }

            if (_promptViewController != null)
                _promptViewController.Hide();

            if (_debugLog)
                Debug.Log($"[InteractionController] 就绪: detector={_triggerDetector != null}, prompt={_promptViewController != null}, tag={tag}");
        }

        private void OnDestroy()
        {
            if (_triggerDetector != null)
            {
                _triggerDetector.OnInteractableEnter -= OnInteractableEnter;
                _triggerDetector.OnInteractableExit -= OnInteractableExit;
            }
        }

        private void OnInteractableEnter(GameObject other)
        {
            _core.SetCurrent(other);
            _core.ShowPrompt();

            string text = InteractableData.ResolvePromptText(other, _config);
            Vector3 offset = _config != null ? _config.promptOffset : new Vector3(0, 1, 0);

            if (_promptViewController != null)
            {
                _promptViewController.SetText(text);
                _promptViewController.SetPosition(other.transform.position, offset);
                _promptViewController.Show();
            }

            if (_debugLog)
                Debug.Log($"[Interaction] Enter: {other.name}, text={text}");
        }

        private void OnInteractableExit(GameObject other)
        {
            if (_core.CurrentInteractable != other) return;

            _core.HidePrompt();
            if (_promptViewController != null)
                _promptViewController.Hide();

            if (_debugLog)
                Debug.Log($"[Interaction] Exit: {other.name}");
        }
    }
}
