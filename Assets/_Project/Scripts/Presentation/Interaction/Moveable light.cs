using UnityEngine;

public class Moveablelight : MonoBehaviour
{
    private enum DragMode
    {
        HorizontalX,
        VerticalY
    }

    private enum ClampSpace
    {
        World,
        Local
    }

    [SerializeField] private Camera dragCamera;
    [SerializeField] private DragMode dragMode = DragMode.HorizontalX;
    [SerializeField, Tooltip("当前模式轴向的最小值：X 模式限制 X，Y 模式限制 Y")]
    private float minLimit = 113.32f;
    [SerializeField, Tooltip("当前模式轴向的最大值：X 模式限制 X，Y 模式限制 Y")]
    private float maxLimit = 123f;
    [SerializeField] private float clickColliderRadius = 0.5f;
    [SerializeField] private ClampSpace clampSpace = ClampSpace.World;

    private float _dragOffsetAxis;
    private float _fixedX;
    private float _fixedY;
    private float _fixedZ;
    private float _fixedLocalX;
    private float _fixedLocalY;
    private float _fixedLocalZ;
    private bool _isDragging;
    private Plane _dragPlane;
    private Collider[] _selfColliders;

    private void Awake()
    {
        if (dragCamera == null)
        {
            dragCamera = Camera.main;
        }

        _fixedX = transform.position.x;
        _fixedY = transform.position.y;
        _fixedZ = transform.position.z;
        _fixedLocalX = transform.localPosition.x;
        _fixedLocalY = transform.localPosition.y;
        _fixedLocalZ = transform.localPosition.z;
        EnsureClickableCollider();
        CacheSelfColliders();
    }

    private void Update()
    {
        if (dragCamera == null)
        {
            dragCamera = Camera.main;
            if (dragCamera == null)
            {
                return;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryBeginDrag();
        }

        if (_isDragging && Input.GetMouseButton(0))
        {
            DragByMouse();
        }

        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }
    }

    private void TryBeginDrag()
    {
        Ray ray = dragCamera.ScreenPointToRay(Input.mousePosition);
        if (!IsRayHittingSelf(ray))
        {
            return;
        }

        _isDragging = true;
        _dragPlane = new Plane(-dragCamera.transform.forward, transform.position);

        if (_dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPointOnDragPlane = ray.GetPoint(enter);
            _dragOffsetAxis = dragMode == DragMode.HorizontalX
                ? transform.position.x - hitPointOnDragPlane.x
                : transform.position.y - hitPointOnDragPlane.y;
            return;
        }

        _isDragging = false;
    }

    private bool IsRayHittingSelf(Ray ray)
    {
        if (_selfColliders == null || _selfColliders.Length == 0)
        {
            CacheSelfColliders();
        }

        for (int i = 0; i < _selfColliders.Length; i++)
        {
            Collider col = _selfColliders[i];
            if (col == null || !col.enabled || !col.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (col.Raycast(ray, out RaycastHit _, 1000f))
            {
                return true;
            }
        }

        return false;
    }

    private void DragByMouse()
    {
        Ray ray = dragCamera.ScreenPointToRay(Input.mousePosition);
        if (!_dragPlane.Raycast(ray, out float enter))
        {
            return;
        }

        Vector3 hitPointOnDragPlane = ray.GetPoint(enter);
        float targetWorldAxis = dragMode == DragMode.HorizontalX
            ? hitPointOnDragPlane.x + _dragOffsetAxis
            : hitPointOnDragPlane.y + _dragOffsetAxis;

        if (clampSpace == ClampSpace.Local && transform.parent != null)
        {
            Vector3 worldTarget = dragMode == DragMode.HorizontalX
                ? new Vector3(targetWorldAxis, _fixedY, _fixedZ)
                : new Vector3(_fixedX, targetWorldAxis, _fixedZ);
            Vector3 localTarget = transform.parent.InverseTransformPoint(worldTarget);

            if (dragMode == DragMode.HorizontalX)
            {
                localTarget.x = Mathf.Clamp(localTarget.x, minLimit, maxLimit);
                transform.localPosition = new Vector3(localTarget.x, _fixedLocalY, _fixedLocalZ);
            }
            else
            {
                localTarget.y = Mathf.Clamp(localTarget.y, minLimit, maxLimit);
                transform.localPosition = new Vector3(_fixedLocalX, localTarget.y, _fixedLocalZ);
            }
            return;
        }

        if (dragMode == DragMode.HorizontalX)
        {
            float clampedWorldX = Mathf.Clamp(targetWorldAxis, minLimit, maxLimit);
            transform.position = new Vector3(clampedWorldX, _fixedY, _fixedZ);
            return;
        }

        float clampedWorldY = Mathf.Clamp(targetWorldAxis, minLimit, maxLimit);
        transform.position = new Vector3(_fixedX, clampedWorldY, _fixedZ);
    }

    private void EnsureClickableCollider()
    {
        if (GetComponent<Collider>() != null)
        {
            return;
        }

        var sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = clickColliderRadius;
    }

    private void CacheSelfColliders()
    {
        _selfColliders = GetComponentsInChildren<Collider>(true);
    }
}
