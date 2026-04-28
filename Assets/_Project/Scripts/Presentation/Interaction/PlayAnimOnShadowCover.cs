using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayAnimOnShadowCover : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("影子物体")]
    private Transform shadowTransform;
    [SerializeField, Tooltip("影子渲染器（无碰撞体时用于尺寸计算）")]
    private Renderer shadowRenderer;
    [SerializeField, Tooltip("要播放动画的 Animator（默认取纸张物体上的 Animator）")]
    private Animator paperAnimator;

    [Header("Detection")]
    [SerializeField, Tooltip("按 XY 平面检测覆盖")]
    private bool xyOnly = true;
    [SerializeField, Tooltip("持续覆盖达到该时长后触发")]
    private float requiredCoverTime = 0.05f;
    [SerializeField, Tooltip("无渲染器时使用的纸张尺寸（世界单位）")]
    private Vector2 paperSizeOverride = Vector2.zero;
    [SerializeField, Tooltip("无渲染器时使用的影子尺寸（世界单位）")]
    private Vector2 shadowSizeOverride = Vector2.zero;

    [Header("Animation")]
    [SerializeField, Tooltip("要播放的动画剪辑")]
    private AnimationClip animationClip;
    [SerializeField, Tooltip("是否允许重复触发")]
    private bool allowRepeat = false;
    [SerializeField, Tooltip("重复触发冷却时间（秒）")]
    private float repeatCooldown = 0.2f;

    private float _coverTimer;
    private float _lastPlayTime = -999f;
    private bool _hasPlayed;
    private bool _isCurrentlyCovered;
    private bool _armed;
    private PlayableGraph _playableGraph;

    private void Awake()
    {
        if (paperAnimator == null)
            paperAnimator = GetComponentInChildren<Animator>(true);
    }

    private void OnDisable()
    {
        if (_playableGraph.IsValid())
            _playableGraph.Destroy();
    }

    private void Update()
    {
        if (paperAnimator == null || shadowTransform == null || animationClip == null)
            return;

        bool covered = IsCoveredByShadow();
        if (!covered)
        {
            _coverTimer = 0f;
            _isCurrentlyCovered = false;
            _armed = true;
            return;
        }

        _isCurrentlyCovered = true;
        if (!_armed)
            return;
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

    private bool IsCoveredByShadow()
    {
        Bounds paperBounds = GetBounds(transform, null, paperSizeOverride);
        Bounds shadowBounds = GetBounds(shadowTransform, shadowRenderer, shadowSizeOverride);

        if (!xyOnly)
            return paperBounds.Intersects(shadowBounds);

        bool overlapX = paperBounds.min.x <= shadowBounds.max.x && paperBounds.max.x >= shadowBounds.min.x;
        bool overlapY = paperBounds.min.y <= shadowBounds.max.y && paperBounds.max.y >= shadowBounds.min.y;
        return overlapX && overlapY;
    }

    private Bounds GetBounds(Transform target, Renderer renderer, Vector2 sizeOverride)
    {
        if (renderer != null)
            return renderer.bounds;

        Vector3 center = target != null ? target.position : Vector3.zero;
        Vector2 size2D = sizeOverride;

        if (size2D == Vector2.zero)
            size2D = Vector2.one * 0.1f;

        return new Bounds(center, new Vector3(size2D.x, size2D.y, 0.01f));
    }

    private void PlayAnimation()
    {
        _hasPlayed = true;
        _lastPlayTime = Time.time;

        if (_playableGraph.IsValid())
            _playableGraph.Destroy();

        _playableGraph = PlayableGraph.Create("ShadowCoverAnim");
        var output = AnimationPlayableOutput.Create(_playableGraph, "AnimOutput", paperAnimator);
        var clipPlayable = AnimationClipPlayable.Create(_playableGraph, animationClip);
        clipPlayable.SetTime(0);
        clipPlayable.SetDuration(animationClip.length);
        output.SetSourcePlayable(clipPlayable);
        _playableGraph.Play();
    }
}
