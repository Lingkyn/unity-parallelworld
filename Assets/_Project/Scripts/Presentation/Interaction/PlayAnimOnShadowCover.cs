using UnityEngine;

public class PlayAnimOnShadowCover : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("灯的物体")]
    private Transform lightTransform;
    [SerializeField, Tooltip("要播放动画的 Animator（默认取纸张物体上的 Animator）")]
    private Animator paperAnimator;

    [Header("Detection")]
    [SerializeField, Tooltip("连续在范围内达到该时长后触发")]
    private float requiredCoverTime = 0.5f;
    [SerializeField, Tooltip("灯进入范围才允许触发")]
    private bool requireLightInRange = true;
    [SerializeField, Tooltip("灯范围最小值（X）")]
    private float lightRangeMinX = -1f;
    [SerializeField, Tooltip("灯范围最大值（X）")]
    private float lightRangeMaxX = 1f;

    [Header("Animation")]
    [SerializeField, Tooltip("要跳转到的 Animator 状态名")]
    private string animationStateName = "";
    [SerializeField, Tooltip("启动时禁用 Animator，避免默认状态自动播放")]
    private bool disableAnimatorOnStart = true;
    [SerializeField, Tooltip("是否允许重复触发")]
    private bool allowRepeat = false;
    [SerializeField, Tooltip("重复触发冷却时间（秒）")]
    private float repeatCooldown = 0.2f;

    private float _coverTimer;
    private float _lastPlayTime = -999f;
    private bool _hasPlayed;
    private bool _isCurrentlyCovered;

    private void Awake()
    {
        if (paperAnimator == null)
            paperAnimator = GetComponentInChildren<Animator>(true);

        if (paperAnimator != null && disableAnimatorOnStart)
            paperAnimator.enabled = false;
    }

    private void Update()
    {
        if (paperAnimator == null || lightTransform == null || string.IsNullOrEmpty(animationStateName))
            return;

        bool inRange = IsLightInRange();

        if (requireLightInRange && !inRange)
        {
            _coverTimer = 0f;
            _isCurrentlyCovered = false;
            return;
        }

        if (disableAnimatorOnStart)
        {
            disableAnimatorOnStart = false;
            if (paperAnimator != null && !paperAnimator.enabled)
                paperAnimator.enabled = true;
        }

        _isCurrentlyCovered = true;
        _coverTimer += Time.deltaTime;

        if (_coverTimer < Mathf.Max(0f, requiredCoverTime))
            return;

        if (!allowRepeat && _hasPlayed)
            return;

        if (Time.time - _lastPlayTime < Mathf.Max(0f, repeatCooldown))
            return;

        PlayAnimation();
    }

    public bool IsCurrentlyCovered
    {
        get { return _isCurrentlyCovered; }
    }

    private bool IsLightInRange()
    {
        if (lightTransform == null)
            return false;

        float minX = Mathf.Min(lightRangeMinX, lightRangeMaxX);
        float maxX = Mathf.Max(lightRangeMinX, lightRangeMaxX);
        float x = lightTransform.position.x;
        return x >= minX && x <= maxX;
    }


    private void PlayAnimation()
    {
        _hasPlayed = true;
        _lastPlayTime = Time.time;

        if (paperAnimator != null && !paperAnimator.enabled)
            paperAnimator.enabled = true;

        paperAnimator.Play(animationStateName, 0, 0f);
    }
}
