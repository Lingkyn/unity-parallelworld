using UnityEngine;
using System.Collections.Generic;

public class BreakOnShadowCover : MonoBehaviour
{
    public enum BreakMode
    {
        OnShadowCover = 0,
        ExternalTriggerOnly = 1
    }

    [Header("References")]
    [SerializeField, Tooltip("宝石自身碰撞体；留空自动获取")]
    private Collider gemCollider;
    [SerializeField, Tooltip("可选：宝石渲染器；无碰撞体时用于覆盖判定")]
    private Renderer gemRenderer;
    [SerializeField, Tooltip("可选：手动指定一个影子碰撞体；留空时自动查找实时生成的影子碰撞体")]
    private Collider shadowCollider;
    [SerializeField, Tooltip("可选：手动指定一个影子渲染器（不依赖碰撞体）")]
    private Renderer shadowRenderer;
    [SerializeField, Tooltip("可选：仅检测该物体（及其子物体）下生成的影子")]
    private Transform shadowSourceRoot;
    [SerializeField, Tooltip("自动查找 SpriteShadow 运行时生成的影子碰撞体")]
    private bool autoFindShadowColliders = true;
    [SerializeField, Tooltip("自动查找刷新间隔（秒）")]
    private float autoFindInterval = 0.4f;

    [Header("Detection")]
    [SerializeField, Tooltip("是否忽略 Z，只按 XY 平面检测覆盖")]
    private bool xyOnly = true;
    [SerializeField, Tooltip("持续覆盖达到该时长后才碎裂")]
    private float requiredCoverTime = 0.05f;
    [SerializeField, Tooltip("碎裂触发模式：默认被影子覆盖后自动碎裂；也可仅记录覆盖并由外部触发")]
    private BreakMode breakMode = BreakMode.OnShadowCover;

    [Header("Break Result")]
    [SerializeField, Tooltip("碎裂时要显示的物体（可选）")]
    private GameObject showOnBreak;
    [SerializeField, Tooltip("碎裂特效预制体（可选）")]
    private GameObject breakVfxPrefab;

    private float _coverTimer;
    private bool _broken;
    private bool _isCurrentlyCovered;
    private bool _hasMetCoverCondition;
    private float _nextAutoFindTime;
    private readonly List<Collider> _autoShadowColliders = new List<Collider>();
    private readonly List<Renderer> _autoShadowRenderers = new List<Renderer>();

    private void Awake()
    {
        if (gemCollider == null)
            gemCollider = GetComponent<Collider>();
        if (gemRenderer == null)
            gemRenderer = GetComponentInChildren<Renderer>(true);

        if (autoFindShadowColliders)
            RefreshAutoShadowColliders();
    }

    private void Update()
    {
        if (_broken)
            return;

        if (gemCollider == null && gemRenderer == null)
            return;

        if (autoFindShadowColliders && Time.time >= _nextAutoFindTime)
            RefreshAutoShadowColliders();

        bool covered = false;

        if (shadowCollider != null && shadowCollider.enabled && shadowCollider.gameObject.activeInHierarchy)
            covered = IsCoveredByShadow(shadowCollider);

        if (!covered && shadowRenderer != null && shadowRenderer.enabled && shadowRenderer.gameObject.activeInHierarchy)
            covered = IsCoveredByShadow(shadowRenderer);

        if (!covered)
            covered = IsCoveredByAnyAutoShadow();

        if (!covered)
        {
            _coverTimer = 0f;
            _isCurrentlyCovered = false;
            return;
        }

        _isCurrentlyCovered = true;
        _coverTimer += Time.deltaTime;
        if (_coverTimer >= Mathf.Max(0f, requiredCoverTime))
        {
            _hasMetCoverCondition = true;

            if (breakMode == BreakMode.OnShadowCover)
                BreakGem();
        }
    }

    public bool IsCurrentlyCovered
    {
        get { return _isCurrentlyCovered; }
    }

    public bool HasMetCoverCondition
    {
        get { return _hasMetCoverCondition; }
    }

    public bool IsBroken
    {
        get { return _broken; }
    }

    public void BreakByExternalTrigger(bool requireCoverCondition)
    {
        if (_broken)
            return;

        if (requireCoverCondition && !_hasMetCoverCondition)
            return;

        BreakGem();
    }

    private bool IsCoveredByAnyAutoShadow()
    {
        for (int i = 0; i < _autoShadowColliders.Count; i++)
        {
            Collider c = _autoShadowColliders[i];
            if (c == null || !c.enabled || !c.gameObject.activeInHierarchy)
                continue;

            if (IsCoveredByShadow(c))
                return true;
        }

        for (int i = 0; i < _autoShadowRenderers.Count; i++)
        {
            Renderer r = _autoShadowRenderers[i];
            if (r == null || !r.enabled || !r.gameObject.activeInHierarchy)
                continue;

            if (IsCoveredByShadow(r))
                return true;
        }

        return false;
    }

    private bool IsCoveredByShadow(Collider activeShadowCollider)
    {
        return IsCoveredByShadowBounds(activeShadowCollider.bounds);
    }

    private bool IsCoveredByShadow(Renderer activeShadowRenderer)
    {
        return IsCoveredByShadowBounds(activeShadowRenderer.bounds);
    }

    private bool IsCoveredByShadowBounds(Bounds shadowBounds)
    {
        Bounds gemBounds = GetGemBounds();

        if (!xyOnly)
            return gemBounds.Intersects(shadowBounds);

        bool overlapX = gemBounds.min.x <= shadowBounds.max.x && gemBounds.max.x >= shadowBounds.min.x;
        bool overlapY = gemBounds.min.y <= shadowBounds.max.y && gemBounds.max.y >= shadowBounds.min.y;
        return overlapX && overlapY;
    }

    private Bounds GetGemBounds()
    {
        if (gemCollider != null)
            return gemCollider.bounds;
        if (gemRenderer != null)
            return gemRenderer.bounds;

        return new Bounds(transform.position, Vector3.one * 0.01f);
    }

    private void RefreshAutoShadowColliders()
    {
        _nextAutoFindTime = Time.time + Mathf.Max(0.05f, autoFindInterval);
        _autoShadowColliders.Clear();
        _autoShadowRenderers.Clear();

        SpriteShadow[] shadows = FindObjectsOfType<SpriteShadow>(true);
        for (int i = 0; i < shadows.Length; i++)
        {
            SpriteShadow s = shadows[i];
            if (s == null || s.lightEntries == null)
                continue;
            if (!IsUnderShadowSourceRoot(s.transform))
                continue;

            for (int j = 0; j < s.lightEntries.Count; j++)
            {
                SpriteShadow.LightShadowEntry entry = s.lightEntries[j];
                if (entry == null || entry.shadowObject == null)
                    continue;

                Collider c = entry.shadowObject.GetComponent<Collider>();
                if (c == null)
                    c = entry.shadowObject.GetComponentInChildren<Collider>(true);

                Renderer r = entry.shadowObject.GetComponent<Renderer>();
                if (r == null)
                    r = entry.shadowObject.GetComponentInChildren<Renderer>(true);

                if (c != null && c != gemCollider)
                    _autoShadowColliders.Add(c);

                if (r != null && r != gemRenderer)
                    _autoShadowRenderers.Add(r);
            }
        }
    }

    private bool IsUnderShadowSourceRoot(Transform candidate)
    {
        if (shadowSourceRoot == null || candidate == null)
            return true;

        return candidate == shadowSourceRoot || candidate.IsChildOf(shadowSourceRoot);
    }

    private void BreakGem()
    {
        _broken = true;

        if (breakVfxPrefab != null)
            Instantiate(breakVfxPrefab, transform.position, Quaternion.identity);

        if (showOnBreak != null)
            showOnBreak.SetActive(true);

        SetGemVisible(false);
    }

    private void SetGemVisible(bool visible)
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = visible;

        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < spriteRenderers.Length; i++)
            spriteRenderers[i].enabled = visible;

        var colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = visible;

        gameObject.SetActive(false);
    }
}
