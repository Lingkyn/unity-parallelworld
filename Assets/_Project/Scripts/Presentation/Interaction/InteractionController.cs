using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 交互系统控制器：整合 TriggerDetector、PromptViewController、InteractButtonViewController
    /// 按 Tag 分支：InteractableButton 显示按钮，点击播放动画；其他显示文本
    /// 多按钮时按距离+表序选最近；播放中锁定玩家
    /// </summary>
    public class InteractionController : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private TriggerDetector _triggerDetector;
        [SerializeField] private PromptViewController _promptViewController;
        [SerializeField] private InteractButtonViewController _interactButtonViewController;
        [SerializeField] private InteractionConfig _config;

        /// <summary>供 InteractableButtonData 等在 Start 时解析表配置使用</summary>
        public InteractionConfig Config => _config;
        [SerializeField] private Camera _camera;
        [SerializeField, Tooltip("调试日志")]
        private bool _debugLog;

        private readonly HashSet<GameObject> _inRange = new HashSet<GameObject>();
        private MovementController _movementController;
        private bool _didFirstFrameHide;
        private Coroutine _unlockCoroutine;

        private PromptViewController[] _promptViewControllers;
        private InteractButtonViewController[] _interactButtonViewControllers;

        private struct CachedInteractable
        {
            public InteractableButtonData ButtonData;
            public InteractableData InteractableData;
        }
        private Dictionary<GameObject, CachedInteractable> _componentCache;

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
            if (_triggerDetector == null)
                Debug.LogWarning("[InteractionController] 未指定 Trigger Detector：在挂有本组件的物体 Inspector 中，将玩家子物体 InteractionDetector 上的 TriggerDetector 拖入对应字段");
            else
            {
                var player = _triggerDetector.transform.parent;
                if (player != null)
                    _movementController = player.GetComponent<MovementController>();
            }
            if (_movementController == null && _triggerDetector != null)
                _movementController = _triggerDetector.GetComponentInParent<MovementController>();
            if (_promptViewController == null)
                Debug.LogWarning("[InteractionController] 未指定 Prompt View Controller：将挂有 PromptViewController 的物体（如 PromptUI）拖入本组件的 Prompt View Controller 字段");
            if (_interactButtonViewController == null && _debugLog)
                Debug.Log("[InteractionController] 未指定 Interact Button View Controller：按钮型交互将不可用；若有 PromptButton 物体可拖入对应字段");

            if (_config != null && _triggerDetector != null)
                _triggerDetector.SetInteractableFilter(_config.interactableLayer);

            _interactButtonViewController?.SetConfig(_config);
            _promptViewController?.SetConfig(_config);
            _componentCache = new Dictionary<GameObject, CachedInteractable>();
        }

        private void Start()
        {
            if (_triggerDetector != null)
            {
                _triggerDetector.OnInteractableEnter += OnInteractableEnter;
                _triggerDetector.OnInteractableExit += OnInteractableExit;
            }

            if (_interactButtonViewController != null)
                _interactButtonViewController.OnClicked += OnInteractButtonClicked;

            _promptViewControllers = FindObjectsByType<PromptViewController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _interactButtonViewControllers = FindObjectsByType<InteractButtonViewController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var vc in _promptViewControllers)
                vc.Hide();
            foreach (var vc in _interactButtonViewControllers)
                vc.Hide();

            if (_debugLog)
                Debug.Log($"[InteractionController] 就绪: detector={_triggerDetector != null}, prompt={_promptViewController != null}, button={_interactButtonViewController != null}");
        }

        private void Update()
        {
            if (!_didFirstFrameHide && Time.frameCount >= 2 && _inRange.Count == 0)
            {
                _didFirstFrameHide = true;
                if (_promptViewControllers != null)
                    foreach (var vc in _promptViewControllers) vc.Hide();
                if (_interactButtonViewControllers != null)
                    foreach (var vc in _interactButtonViewControllers) vc.Hide();
            }
        }

        private void OnDestroy()
        {
            if (_triggerDetector != null)
            {
                _triggerDetector.OnInteractableEnter -= OnInteractableEnter;
                _triggerDetector.OnInteractableExit -= OnInteractableExit;
            }
            if (_interactButtonViewController != null)
                _interactButtonViewController.OnClicked -= OnInteractButtonClicked;
        }

        private void OnInteractableEnter(GameObject other)
        {
            if (other != null)
            {
                _inRange.Add(other);
                _componentCache[other] = new CachedInteractable
                {
                    ButtonData = other.GetComponent<InteractableButtonData>(),
                    InteractableData = other.GetComponent<InteractableData>()
                };
            }
            RefreshCurrentTarget();
        }

        private void OnInteractableExit(GameObject other)
        {
            _inRange.Remove(other);
            if (other != null)
                _componentCache.Remove(other);
            RefreshCurrentTarget();
        }

        /// <summary>
        /// 从范围内选出最佳可交互物：按钮型按距离+表序，文本型取第一个；同时支持文本+按钮
        /// </summary>
        private void RefreshCurrentTarget()
        {
            if (_inRange.Count == 0)
            {
                if (_promptViewControllers != null)
                    foreach (var vc in _promptViewControllers) vc.Hide();
                if (_interactButtonViewControllers != null)
                    foreach (var vc in _interactButtonViewControllers) vc.Hide();
                return;
            }

            Vector3 triggerPos = _triggerDetector != null ? _triggerDetector.transform.position : Vector3.zero;
            var db = _config?.buttonDatabase;

            // 分离按钮型与文本型（用 sqrMagnitude 避免 sqrt 开销）
            GameObject bestButton = null;
            float bestButtonDistSq = float.MaxValue;
            int bestButtonPriority = int.MaxValue;

            GameObject bestText = null;

            foreach (var go in _inRange)
            {
                if (go == null) continue;
                if (!_componentCache.TryGetValue(go, out var cached)) continue;

                if (go.CompareTag(Tags.InteractableButton))
                {
                    var b = cached.ButtonData;
                    if (b == null || !b.HasAnimation(db) || b.IsAnimationFinished()) continue;

                    float distSq = (triggerPos - go.transform.position).sqrMagnitude;
                    int prio = b.GetPriority(db);
                    // 同距离：表模式按表序，Local 模式按 GetInstanceID 稳定排序
                    int bestId = bestButton != null ? bestButton.GetInstanceID() : 0;
                    bool isBetter = distSq < bestButtonDistSq
                        || (Mathf.Approximately(distSq, bestButtonDistSq) && (prio < bestButtonPriority || (prio == bestButtonPriority && go.GetInstanceID() < bestId)));
                    if (isBetter)
                    {
                        bestButtonDistSq = distSq;
                        bestButtonPriority = prio;
                        bestButton = go;
                    }
                }
                else if (bestText == null)
                {
                    bestText = go;
                }
            }

            // 显示按钮型
            if (bestButton != null && _componentCache.TryGetValue(bestButton, out var bestButtonCached))
            {
                if (_interactButtonViewController == null && _debugLog)
                    Debug.Log("[Interaction] bestButton found but _interactButtonViewController=null");
                _promptViewController?.Hide();

                var buttonData = bestButtonCached.ButtonData;
                Vector3 offset = _config != null ? _config.promptOffset : new Vector3(0, 1, 0);

                _interactButtonViewController?.SetButtonText(buttonData.ResolveButtonText(db));
                _interactButtonViewController?.SetPosition(bestButton.transform.position, offset);
                _interactButtonViewController?.Show(bestButton);

                // 同一物体既有文本又有按钮：文本在上（使用缓存的 textData 避免 GetComponent）
                var textData = bestButtonCached.InteractableData;
                if (textData != null && _promptViewController != null)
                {
                    string text = InteractableData.ResolvePromptText(textData, _config);
                    _promptViewController.SetText(text);
                    _promptViewController.SetPosition(bestButton.transform.position, offset + new Vector3(0, 0.3f, 0));
                    _promptViewController.Show(bestButton);
                }

                if (_debugLog)
                    Debug.Log($"[Interaction] 显示按钮: {bestButton.name}");
            }
            else if (bestText != null)
            {
                _interactButtonViewController?.Hide();

                string text = _componentCache.TryGetValue(bestText, out var bestTextCached) && bestTextCached.InteractableData != null
                    ? InteractableData.ResolvePromptText(bestTextCached.InteractableData, _config)
                    : InteractableData.ResolvePromptText(bestText, _config);
                Vector3 offset = _config != null ? _config.promptOffset : new Vector3(0, 1, 0);
                _promptViewController?.SetText(text);
                _promptViewController?.SetPosition(bestText.transform.position, offset);
                _promptViewController?.Show(bestText);

                if (_debugLog)
                    Debug.Log($"[Interaction] 显示文本: {bestText.name}, text={text}");
            }
            else
            {
                _promptViewController?.Hide();
                _interactButtonViewController?.Hide();
            }
        }

        private void OnInteractButtonClicked(GameObject target)
        {
            if (target == null) return;
            if (!_componentCache.TryGetValue(target, out var cached))
            {
                var fallback = target.GetComponent<InteractableButtonData>();
                if (fallback == null || !fallback.HasAnimation(_config?.buttonDatabase)) return;
                cached = new CachedInteractable { ButtonData = fallback, InteractableData = null };
            }
            var buttonData = cached.ButtonData;
            if (buttonData == null || !buttonData.HasAnimation(_config?.buttonDatabase)) return;

            // 点击后立即隐藏按钮
            _interactButtonViewController?.Hide();
            _promptViewController?.Hide();

            buttonData.PlayAnimation(_config?.buttonDatabase);

            // 播放中锁定玩家
            LockPlayer();
            var clip = buttonData.ResolveAnimationClip(_config?.buttonDatabase);
            float duration = clip != null ? clip.length : 1f;
            if (_unlockCoroutine != null)
                StopCoroutine(_unlockCoroutine);
            _unlockCoroutine = StartCoroutine(UnlockAfterDelay(duration));

            if (_debugLog)
                Debug.Log($"[Interaction] Button clicked: {target.name}, 播放动画 {duration}s");
        }

        private void LockPlayer()
        {
            if (_movementController != null)
                _movementController.enabled = false;
        }

        private void UnlockPlayer()
        {
            if (_movementController != null)
                _movementController.enabled = true;
        }

        private IEnumerator UnlockAfterDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            UnlockPlayer();
            _unlockCoroutine = null;
            RefreshCurrentTarget();
        }
    }
}
