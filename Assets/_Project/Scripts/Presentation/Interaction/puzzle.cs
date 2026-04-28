using UnityEngine;
using System.Collections.Generic;

public class puzzle : MonoBehaviour
{
    public bool IsSequenceFinished => _hasMotionFinished;

    [Header("条件检测")]
    [SerializeField, Tooltip("箱子需要到达的平台触发区域")]
    private Collider boxTargetArea;
    [SerializeField, Tooltip("玩家需要站上的平台触发区域")]
    private Collider playerTargetArea;
    [SerializeField, Tooltip("要检测的箱子碰撞体")]
    private Collider boxCollider;
    [SerializeField, Tooltip("可选：指定一个玩家碰撞体；留空时自动检测 Player 下所有碰撞体（含 Real/Shadow）")]
    private Collider playerCollider;

    [Header("联动下降")]
    [SerializeField, Tooltip("需要一起下降的 3 个物体")]
    private Transform[] objectsToLower = new Transform[3];
    [SerializeField, Tooltip("下降距离")]
    private float lowerDistance = 2f;
    [SerializeField, Tooltip("下降速度")]
    private float lowerSpeed = 2f;
    [SerializeField, Tooltip("需要一起上升的物体（可增减）")]
    private Transform[] objectsToRaise = new Transform[2];
    [SerializeField, Tooltip("上升距离")]
    private float raiseDistance = 2f;
    [Header("完成后切换")]
    [SerializeField, Tooltip("当升降全部完成后出现的物体（SetActive(true)）")]
    private GameObject showAfterSequence;
    [SerializeField, Tooltip("当升降全部完成后消失的物体（SetActive(false)）")]
    private GameObject hideAfterSequence;
    [SerializeField, Tooltip("是否只触发一次")]
    private bool triggerOnce = true;

    private Vector3[] _startPositions;
    private Vector3[] _targetPositions;
    private Vector3[] _raiseTargetPositions;
    private bool _isLowering;
    private bool _hasTriggered;
    private bool _hasMotionFinished;
    private readonly List<Collider> _playerColliders = new List<Collider>();
    private float _nextPlayerCacheTime;

    private void Awake()
    {
        CachePlayerColliders();
    }

    private void Update()
    {
        if (!_isLowering)
            TryStartLowering();

        if (_isLowering)
            LowerObjectsStep();

        if (playerCollider == null && Time.time >= _nextPlayerCacheTime && _playerColliders.Count == 0)
            CachePlayerColliders();
    }

    private void TryStartLowering()
    {
        if (_hasTriggered && triggerOnce)
            return;
        if (!HasValidSetup())
            return;

        bool boxReady = IsOverlapping(boxTargetArea, boxCollider);
        bool playerReady = IsPlayerOnTargetArea();
        if (!boxReady || !playerReady)
            return;

        _hasMotionFinished = false;

        _startPositions = new Vector3[objectsToLower.Length];
        _targetPositions = new Vector3[objectsToLower.Length];
        for (int i = 0; i < objectsToLower.Length; i++)
        {
            Transform t = objectsToLower[i];
            if (t == null)
                continue;

            _startPositions[i] = t.position;
            _targetPositions[i] = t.position + Vector3.down * lowerDistance;
        }

        _raiseTargetPositions = new Vector3[objectsToRaise.Length];
        for (int i = 0; i < objectsToRaise.Length; i++)
        {
            Transform t = objectsToRaise[i];
            if (t == null)
                continue;

            _raiseTargetPositions[i] = t.position + Vector3.up * raiseDistance;
        }

        _isLowering = true;
        _hasTriggered = true;
    }

    private void LowerObjectsStep()
    {
        bool allArrived = true;

        for (int i = 0; i < objectsToLower.Length; i++)
        {
            Transform t = objectsToLower[i];
            if (t == null)
                continue;

            Vector3 target = _targetPositions[i];
            Vector3 next = Vector3.MoveTowards(t.position, target, lowerSpeed * Time.deltaTime);
            t.position = next;

            if ((target - next).sqrMagnitude > 0.0001f)
                allArrived = false;
        }

        for (int i = 0; i < objectsToRaise.Length; i++)
        {
            Transform t = objectsToRaise[i];
            if (t == null)
                continue;

            Vector3 target = _raiseTargetPositions[i];
            Vector3 next = Vector3.MoveTowards(t.position, target, lowerSpeed * Time.deltaTime);
            t.position = next;

            if ((target - next).sqrMagnitude > 0.0001f)
                allArrived = false;
        }

        if (allArrived)
        {
            _isLowering = false;
            _hasMotionFinished = true;
            ApplyCompletionToggles();
        }
    }

    private void ApplyCompletionToggles()
    {
        if (showAfterSequence != null)
            showAfterSequence.SetActive(true);

        if (hideAfterSequence != null)
            hideAfterSequence.SetActive(false);
    }

    private bool HasValidSetup()
    {
        if (boxTargetArea == null || playerTargetArea == null)
            return false;
        if (boxCollider == null)
            return false;
        if (playerCollider == null && _playerColliders.Count == 0)
            return false;
        if (objectsToLower == null || objectsToLower.Length == 0)
            return false;

        return true;
    }

    private static bool IsOverlapping(Collider area, Collider target)
    {
        if (area == null || target == null)
            return false;

        Bounds a = area.bounds;
        Bounds b = target.bounds;

        bool overlapX = a.min.x <= b.max.x && a.max.x >= b.min.x;
        bool overlapY = a.min.y <= b.max.y && a.max.y >= b.min.y;
        return overlapX && overlapY;
    }

    private bool IsPlayerOnTargetArea()
    {
        if (playerTargetArea == null)
            return false;

        if (playerCollider != null)
            return IsOverlapping(playerTargetArea, playerCollider);

        for (int i = 0; i < _playerColliders.Count; i++)
        {
            Collider col = _playerColliders[i];
            if (col == null || !col.enabled || !col.gameObject.activeInHierarchy)
                continue;

            if (IsOverlapping(playerTargetArea, col))
                return true;
        }

        return false;
    }

    private void CachePlayerColliders()
    {
        _nextPlayerCacheTime = Time.time + 1f;
        _playerColliders.Clear();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        Collider[] cols = player.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            Collider col = cols[i];
            if (col == null)
                continue;
            if (!col.enabled || !col.gameObject.activeInHierarchy)
                continue;

            _playerColliders.Add(col);
        }
    }
}
