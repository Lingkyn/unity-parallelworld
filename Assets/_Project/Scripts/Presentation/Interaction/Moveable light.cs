using UnityEngine;

public class Moveablelight : MonoBehaviour
{
    private enum ClampSpace
    {
        World,
        Local
    }

    [SerializeField] private Camera dragCamera;
    [SerializeField] private float minX = 113.32f;
    [SerializeField] private float maxX = 123f;
    [SerializeField] private float clickColliderRadius = 0.5f;
    [SerializeField] private ClampSpace clampSpace = ClampSpace.World;

    private float _dragOffsetX;
    private float _fixedY;
    private float _fixedZ;
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

        _fixedY = transform.position.y;
        _fixedZ = transform.position.z;
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
            _dragOffsetX = transform.position.x - hitPointOnDragPlane.x;
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
        float targetWorldX = hitPointOnDragPlane.x + _dragOffsetX;

        if (clampSpace == ClampSpace.Local && transform.parent != null)
        {
            Vector3 worldTarget = new Vector3(targetWorldX, _fixedY, _fixedZ);
            Vector3 localTarget = transform.parent.InverseTransformPoint(worldTarget);
            localTarget.x = Mathf.Clamp(localTarget.x, minX, maxX);
            transform.localPosition = new Vector3(localTarget.x, _fixedLocalY, _fixedLocalZ);
            return;
        }

        float clampedWorldX = Mathf.Clamp(targetWorldX, minX, maxX);
        transform.position = new Vector3(clampedWorldX, _fixedY, _fixedZ);
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
