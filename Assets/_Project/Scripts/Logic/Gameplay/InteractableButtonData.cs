using System.Collections;
using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 按钮型可交互物：靠近显示按钮，点击播放动画
    /// 挂于带 Collider 的物体上，需设置 Tag 为 InteractableButton、Layer 为 Interactable
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableButtonData : MonoBehaviour
    {
        public enum ButtonSource
        {
            [UnityEngine.Tooltip("从 InteractableButtonDatabase 按 entryId 查找")]
            Table,
            [UnityEngine.Tooltip("使用组件自有的 buttonText、animationClip")]
            Local
        }

        [SerializeField] private ButtonSource _source = ButtonSource.Local;
        [SerializeField, UnityEngine.Tooltip("表模式时使用，如 door、lever")]
        private string _entryId = "";
        [SerializeField, UnityEngine.Tooltip("本地模式时使用")]
        private string _buttonText = "互动";
        [SerializeField, UnityEngine.Tooltip("播放动画的 Animator")]
        private Animator _animator;
        [SerializeField, UnityEngine.Tooltip("本地模式时使用；表模式从 Database 获取")]
        private AnimationClip _animationClip;
        [SerializeField, UnityEngine.Tooltip("本地模式时使用；表模式从 Database 获取。场景加载时强制进入的状态名，用于避免 Animator 默认状态为动画导致一进场景就播放")]
        private string _defaultStateName = "";
        [SerializeField, UnityEngine.Tooltip("表模式时必填，用于解析默认状态、按钮文本、动画等")]
        private InteractionConfig _config;

        private bool _isFinished;
        private Coroutine _finishCoroutine;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            var db = _config?.buttonDatabase;
            if (_source == ButtonSource.Table && db == null)
                Debug.LogWarning($"[InteractableButtonData] {gameObject.name} 表模式需指定 Config：选中该物体，在 Interactable Button Data 组件的 Config 字段拖入 InteractionConfig.asset");
            var stateName = ResolveDefaultStateName(db);
            // 修复：Animator Controller 默认状态若为动画态会一进场景就播放，此处强制进入指定状态
            if (_animator != null && !string.IsNullOrEmpty(stateName) && _animator.layerCount > 0)
            {
                _animator.Play(stateName, 0, 0f);
            }
        }

        public ButtonSource Source => _source;
        public string EntryId => _entryId ?? "";
        public string ButtonText => _buttonText ?? "互动";
        public Animator Animator => _animator;
        public AnimationClip AnimationClip => _animationClip;

        /// <summary>
        /// 解析按钮文本：表模式查表，否则本地
        /// </summary>
        public string ResolveButtonText(InteractableButtonDatabase database)
        {
            if (_source == ButtonSource.Table && database != null && !string.IsNullOrEmpty(_entryId))
            {
                var entry = database.GetEntry(_entryId);
                if (entry != null && !string.IsNullOrEmpty(entry.buttonText))
                    return entry.buttonText;
            }
            return _buttonText ?? "互动";
        }

        /// <summary>
        /// 解析默认状态名：表模式查表，否则本地
        /// </summary>
        public string ResolveDefaultStateName(InteractableButtonDatabase database)
        {
            if (_source == ButtonSource.Table && database != null && !string.IsNullOrEmpty(_entryId))
            {
                var entry = database.GetEntry(_entryId);
                if (entry != null && !string.IsNullOrEmpty(entry.defaultStateName))
                    return entry.defaultStateName;
            }
            return _defaultStateName ?? "";
        }

        /// <summary>
        /// 解析动画：表模式查表，否则本地
        /// </summary>
        public AnimationClip ResolveAnimationClip(InteractableButtonDatabase database)
        {
            if (_source == ButtonSource.Table && database != null && !string.IsNullOrEmpty(_entryId))
            {
                var entry = database.GetEntry(_entryId);
                if (entry != null && entry.animationClip != null)
                    return entry.animationClip;
            }
            return _animationClip;
        }

        /// <summary>
        /// 表模式时获取优先级，用于同距离排序
        /// </summary>
        public int GetPriority(InteractableButtonDatabase database)
        {
            if (_source != ButtonSource.Table || database == null || string.IsNullOrEmpty(_entryId))
                return int.MaxValue;
            return database.GetPriority(_entryId);
        }

        /// <summary>
        /// 是否有有效动画（表模式需传入 database）
        /// </summary>
        public bool HasAnimation(InteractableButtonDatabase database = null)
        {
            return _animator != null && ResolveAnimationClip(database) != null;
        }

        /// <summary>
        /// 是否已播完，播完后不再显示按钮
        /// </summary>
        public bool IsAnimationFinished()
        {
            return _isFinished;
        }

        /// <summary>
        /// 播放配置的动画，播完后标记已播完
        /// </summary>
        public void PlayAnimation(InteractableButtonDatabase database)
        {
            if (_animator == null) return;

            var clip = ResolveAnimationClip(database);
            if (clip == null) return;

            if (_finishCoroutine != null)
            {
                StopCoroutine(_finishCoroutine);
                _finishCoroutine = null;
            }

            _animator.Play(clip.name);
            _finishCoroutine = StartCoroutine(WaitForAnimationEnd(clip.length));
        }

        private IEnumerator WaitForAnimationEnd(float duration)
        {
            yield return new WaitForSeconds(duration);
            _isFinished = true;
            _finishCoroutine = null;
        }

        private void OnDisable()
        {
            if (_finishCoroutine != null)
            {
                StopCoroutine(_finishCoroutine);
                _finishCoroutine = null;
            }
        }
    }
}
