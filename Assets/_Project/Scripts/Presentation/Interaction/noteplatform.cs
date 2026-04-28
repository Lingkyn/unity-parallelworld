using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class noteplatform : MonoBehaviour
{
    [Header("State Sprites")]
    [SerializeField, Tooltip("要切换的 SpriteRenderer；留空自动找当前物体或子物体 SpriteRenderer")]
    private SpriteRenderer targetSpriteRenderer;
    [SerializeField, Tooltip("状态一：全新 Sprite")]
    private Sprite freshSprite;
    [SerializeField, Tooltip("状态二：半破裂 Sprite")]
    private Sprite crackedSprite;
    [SerializeField, Tooltip("状态三时是否直接隐藏整个物体；否则仅关闭渲染与碰撞")]
    private bool hideWholeObjectOnEmpty = false;

    [Header("Step Logic")]
    [SerializeField, Tooltip("平台启用后的防抖时间，避免激活瞬间被同一帧重复判定")]
    private float armDelay = 0.5f;
    [SerializeField, Tooltip("状态二再次触发后到消失的延迟秒数")]
    private float crackedToEmptyDelay = 0.5f;

    [Header("Player Detect")]
    [SerializeField, Tooltip("刷新玩家碰撞体缓存的间隔")]
    private float refreshPlayersInterval = 0.5f;
    [SerializeField, Tooltip("边缘接触容差，避免只贴边时漏判")]
    private float touchTolerance = 0.03f;

    [Header("Shadow Proxy")]
    [SerializeField, Tooltip("影子形态与真实平台之间允许的 XY 平面距离")]
    private float shadowProxyPlanarDistance = 0.4f;
    [SerializeField, Tooltip("影子形态与真实平台之间允许的最大 Z 差")]
    private float shadowProxyMaxZDistance = 15f;

    [Header("Linked Gem")]
    [SerializeField, Tooltip("可选：联动的宝石碎裂器。平台第二次踩踏后变空时可同步触发宝石消失")]
    private BreakOnShadowCover linkedGemBreaker;
    [SerializeField, Tooltip("为真时，仅当宝石已满足“被影子遮住”条件才会被联动消失")]
    private bool requireShadowCoverBeforeConsumeGem = true;

    private Collider _selfCollider;
    private readonly List<CharacterController> _playerBodies = new List<CharacterController>();
    private float _nextRefreshTime;
    private float _armUntilTime;
    private PlatformState _state;
    private bool _wasTouching;
    private bool _isWaitingToEmpty;

    private enum PlatformState
    {
        Fresh = 0,
        Cracked = 1,
        Empty = 2
    }

    private void Awake()
    {
        _selfCollider = GetComponent<Collider>();
        if (targetSpriteRenderer == null)
            targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        _state = PlatformState.Fresh;
        ApplyVisualState();
        ApplyCollisionState();
        RefreshPlayers();
    }

    private void OnEnable()
    {
        _armUntilTime = Time.time + Mathf.Max(0f, armDelay);
        _wasTouching = true;
    }

    private void FixedUpdate()
    {
        if (_state == PlatformState.Empty || _selfCollider == null)
            return;

        if (Time.time >= _nextRefreshTime)
            RefreshPlayers();

        bool touchingPlayer = IsTouchingAnyPlayer();
        if (Time.time < _armUntilTime)
        {
            _wasTouching = touchingPlayer;
            return;
        }

        if (touchingPlayer && !_wasTouching)
            ConsumePlatform();

        _wasTouching = touchingPlayer;
    }

    private void RefreshPlayers()
    {
        _nextRefreshTime = Time.time + Mathf.Max(0.2f, refreshPlayersInterval);
        _playerBodies.Clear();

        GameObject[] players = GameObject.FindGameObjectsWithTag(Tags.Player);
        for (int i = 0; i < players.Length; i++)
        {
            GameObject player = players[i];
            if (player == null)
                continue;

            _playerBodies.AddRange(player.GetComponentsInChildren<CharacterController>(true));
        }
    }

    private bool IsTouchingAnyPlayer()
    {
        for (int i = 0; i < _playerBodies.Count; i++)
        {
            CharacterController body = _playerBodies[i];
            if (body == null || !body.enabled || !body.gameObject.activeInHierarchy)
                continue;

            if (IsRealOverlap(body))
                return true;

            if (IsShadowBody(body) && IsShadowProxyStepEligible(body))
                return true;
        }

        return false;
    }

    private bool IsRealOverlap(CharacterController body)
    {
        if (Physics.ComputePenetration(
                _selfCollider,
                _selfCollider.transform.position,
                _selfCollider.transform.rotation,
                body,
                body.transform.position,
                body.transform.rotation,
                out _,
                out _))
        {
            return true;
        }

        Bounds expandedBodyBounds = body.bounds;
        expandedBodyBounds.Expand(Mathf.Max(0.001f, touchTolerance));
        return _selfCollider.bounds.Intersects(expandedBodyBounds);
    }

    private bool IsShadowProxyStepEligible(CharacterController body)
    {
        Vector3 playerPos = body.transform.position;
        Vector3 closest = _selfCollider.ClosestPoint(playerPos);

        float deltaZ = Mathf.Abs(playerPos.z - closest.z);
        if (deltaZ > Mathf.Max(0.01f, shadowProxyMaxZDistance))
            return false;

        Vector2 playerXY = new Vector2(playerPos.x, playerPos.y);
        Vector2 platformXY = new Vector2(closest.x, closest.y);
        float planarDistance = Vector2.Distance(playerXY, platformXY);
        return planarDistance <= Mathf.Max(0.01f, shadowProxyPlanarDistance);
    }

    private static bool IsShadowBody(CharacterController body)
    {
        return body != null && body.name.IndexOf("shadow", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void ConsumePlatform()
    {
        if (_state == PlatformState.Fresh)
        {
            _state = PlatformState.Cracked;
            ApplyVisualState();
            ApplyCollisionState();
        }
        else if (_state == PlatformState.Cracked && !_isWaitingToEmpty)
        {
            StartCoroutine(TransitionToEmptyAfterDelay());
        }
    }

    private IEnumerator TransitionToEmptyAfterDelay()
    {
        _isWaitingToEmpty = true;
        yield return new WaitForSeconds(Mathf.Max(0f, crackedToEmptyDelay));

        if (_state == PlatformState.Cracked)
        {
            _state = PlatformState.Empty;
            TryConsumeLinkedGem();
            ApplyVisualState();
            ApplyCollisionState();
        }

        _isWaitingToEmpty = false;
    }

    private void OnDisable()
    {
        _isWaitingToEmpty = false;
        StopAllCoroutines();
    }

    private void ApplyVisualState()
    {
        if (_state == PlatformState.Empty)
        {
            if (hideWholeObjectOnEmpty)
            {
                gameObject.SetActive(false);
                return;
            }

            if (targetSpriteRenderer != null)
                targetSpriteRenderer.enabled = false;
            return;
        }

        if (targetSpriteRenderer == null)
            return;

        targetSpriteRenderer.enabled = true;

        Sprite targetSprite = _state == PlatformState.Fresh ? freshSprite : crackedSprite;
        if (targetSprite != null)
            targetSpriteRenderer.sprite = targetSprite;
    }

    private void ApplyCollisionState()
    {
        if (_selfCollider == null)
            return;

        _selfCollider.enabled = _state != PlatformState.Empty;
    }

    private void TryConsumeLinkedGem()
    {
        if (linkedGemBreaker == null)
            return;

        linkedGemBreaker.BreakByExternalTrigger(requireShadowCoverBeforeConsumeGem);
    }
}
