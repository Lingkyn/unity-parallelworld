using UnityEngine;
using System.Collections.Generic;
using System;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class push : MonoBehaviour
{
    [SerializeField, Tooltip("按 D 推动时物体向右移动速度")]
    private float moveRightSpeed = 6f;
    [SerializeField, Tooltip("碰到指定目标后让该物体消失")]
    private bool disappearOnContact = true;
    [SerializeField, Tooltip("本物体消失时要出现的物体（将被 SetActive(true)）")]
    private GameObject appearOnDisappear;
    [SerializeField, Tooltip("可选：当关联 puzzle 已完成下降流程后，本物体消失时出现的物体")]
    private GameObject appearOnDisappearAfterPuzzleFinished;
    [SerializeField, Tooltip("可选：关联 puzzle 控制器（用于判断是否已完成下降）")]
    private puzzle puzzleStateSource;
    [SerializeField, Tooltip("指定碰到这个 Collider 就消失")]
    private Collider disappearTargetCollider;

    private const float RefreshInterval = 0.5f;
    private const float TouchTolerance = 0.03f;
    private const bool AutoUnfreezeX = true;
    private const bool AllowShadowProxyPush = true;
    private const float ShadowProxyPlanarDistance = 0.35f;
    private const float ShadowProxyMinForwardX = -0.05f;
    private const float ShadowProxyMaxZDistance = 15f;
    private const float SustainedPushGraceTime = 0.2f;
    private const float SustainedPlanarDistanceMultiplier = 1.8f;

    private Rigidbody _rb;
    private Collider _selfCollider;
    private readonly List<CharacterController> _playerBodies = new List<CharacterController>();
    private float _nextRefreshTime;
    private CharacterController _latchedBody;
    private float _pushLatchExpireTime;
    private bool _isDisappeared;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _selfCollider = GetComponent<Collider>();
        if (_rb != null)
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (_rb != null && AutoUnfreezeX && _rb.constraints.HasFlag(RigidbodyConstraints.FreezePositionX))
            _rb.constraints &= ~RigidbodyConstraints.FreezePositionX;

        RefreshPlayers();
    }

    private void FixedUpdate()
    {
        if (_rb == null || _selfCollider == null)
            return;

        if (Time.time >= _nextRefreshTime)
            RefreshPlayers();

        bool moveRightPressed = IsMoveRightPressed();
        bool isTouchingPlayer = IsPushConditionMet(out CharacterController touchedBody);

        if (!isTouchingPlayer && moveRightPressed && IsLatchActive())
            isTouchingPlayer = IsLatchedBodyStillEligible();

        if (!isTouchingPlayer)
            return;

        if (!moveRightPressed)
        {
            ClearLatch();
            return;
        }

        if (touchedBody != null)
            _latchedBody = touchedBody;
        _pushLatchExpireTime = Time.time + Mathf.Max(0.01f, SustainedPushGraceTime);

        float deltaX = moveRightSpeed * Time.fixedDeltaTime;
        Vector3 target = _rb.position;
        target.x += deltaX;

        _rb.WakeUp();
        _rb.MovePosition(target);
    }

    private void RefreshPlayers()
    {
        _nextRefreshTime = Time.time + Mathf.Max(0.2f, RefreshInterval);
        _playerBodies.Clear();

        var players = GameObject.FindGameObjectsWithTag(Tags.Player);
        foreach (var player in players)
        {
            if (player == null) continue;
            _playerBodies.AddRange(player.GetComponentsInChildren<CharacterController>(true));
        }
    }

    private bool IsPushConditionMet(out CharacterController touchedBody)
    {
        touchedBody = null;

        foreach (var body in _playerBodies)
        {
            if (body == null || !body.enabled || !body.gameObject.activeInHierarchy)
                continue;

            if (IsOverlapping(body))
            {
                touchedBody = body;
                return true;
            }

            if (!AllowShadowProxyPush)
                continue;

            if (!IsShadowBody(body))
                continue;

            if (!IsShadowProxyPushEligible(body))
                continue;

            touchedBody = body;
            return true;
        }

        return false;
    }

    private bool IsShadowProxyPushEligible(CharacterController body)
    {
        return IsShadowProxyPushEligible(body, 1f);
    }

    private bool IsShadowProxyPushEligible(CharacterController body, float planarDistanceMultiplier)
    {
        Vector3 playerPos = body.transform.position;
        Vector3 targetPos = _selfCollider.bounds.center;

        float deltaX = targetPos.x - playerPos.x;
        if (deltaX < ShadowProxyMinForwardX)
            return false;

        float deltaZ = Mathf.Abs(targetPos.z - playerPos.z);
        if (deltaZ > Mathf.Max(0.01f, ShadowProxyMaxZDistance))
            return false;

        Vector2 playerXY = new Vector2(playerPos.x, playerPos.y);
        Vector3 closest = _selfCollider.ClosestPoint(playerPos);
        Vector2 closestXY = new Vector2(closest.x, closest.y);
        float planarDistance = Vector2.Distance(playerXY, closestXY);

        float allowedDistance = Mathf.Max(0.01f, ShadowProxyPlanarDistance * Mathf.Max(1f, planarDistanceMultiplier));
        return planarDistance <= allowedDistance;
    }

    private static bool IsShadowBody(CharacterController body)
    {
        return body != null
               && body.name.IndexOf("shadow", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private bool IsOverlapping(CharacterController body)
    {
        if (body == null || _selfCollider == null)
            return false;

        bool overlapped = Physics.ComputePenetration(
            _selfCollider,
            _selfCollider.transform.position,
            _selfCollider.transform.rotation,
            body,
            body.transform.position,
            body.transform.rotation,
            out _,
            out _);

        if (overlapped)
            return true;

        // ComputePenetration 在仅边缘接触时可能返回 false，这里用轻微扩张的 bounds 兜底。
        Bounds expandedBodyBounds = body.bounds;
        expandedBodyBounds.Expand(Mathf.Max(0.001f, TouchTolerance));
        return _selfCollider.bounds.Intersects(expandedBodyBounds);
    }

    private static bool IsMoveRightPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
            return Keyboard.current.dKey.isPressed;
#endif
        return Input.GetKey(KeyCode.D);
    }

    private bool IsLatchActive()
    {
        return _latchedBody != null && Time.time <= _pushLatchExpireTime;
    }

    private bool IsLatchedBodyStillEligible()
    {
        if (_latchedBody == null || !_latchedBody.enabled || !_latchedBody.gameObject.activeInHierarchy)
            return false;

        if (IsOverlapping(_latchedBody))
            return true;

        if (!AllowShadowProxyPush)
            return false;

        if (!IsShadowBody(_latchedBody))
            return false;

        return IsShadowProxyPushEligible(_latchedBody, SustainedPlanarDistanceMultiplier);
    }

    private void ClearLatch()
    {
        _latchedBody = null;
        _pushLatchExpireTime = 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null)
            return;

        TryDisappearByContact(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDisappearByContact(other);
    }

    private void TryDisappearByContact(Collider other)
    {
        if (_isDisappeared || !disappearOnContact || other == null)
            return;

        if (!IsDisappearTarget(other))
            return;

        _isDisappeared = true;
        ActivateAppearTarget();
        gameObject.SetActive(false);
    }

    private void ActivateAppearTarget()
    {
        bool puzzleFinished = puzzleStateSource != null && puzzleStateSource.IsSequenceFinished;

        if (puzzleFinished && appearOnDisappearAfterPuzzleFinished != null)
        {
            appearOnDisappearAfterPuzzleFinished.SetActive(true);
            return;
        }

        if (appearOnDisappear != null)
            appearOnDisappear.SetActive(true);
    }

    private bool IsDisappearTarget(Collider other)
    {
        if (disappearTargetCollider != null)
        {
            if (other == disappearTargetCollider)
                return true;

            if (other.transform.IsChildOf(disappearTargetCollider.transform)
                || disappearTargetCollider.transform.IsChildOf(other.transform))
                return true;
        }

        return false;
    }
}
