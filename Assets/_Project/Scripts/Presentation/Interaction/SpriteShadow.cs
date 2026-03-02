using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteShadow : MonoBehaviour
{
    [Header("Refs")]
    public Transform lightTransform;
    public Transform shadowObject;
    public LayerMask wallMask;

    [Header("Shadow Settings")]
    public float shadowExpand = 0.8f;
    public float shadowThickness = 0.8f;

    [Header("Performance")]
    [Tooltip("Mesh update interval (1=every frame)")]
    public int meshUpdateInterval = 2;
    [Tooltip("BoxCollider update interval")]
    public int colliderUpdateInterval = 4;

    // cached refs
    private SpriteRenderer sr;
    private Mesh shadowMesh;
    private BoxCollider shadowCollider; // lazy created
    private Light _lightComp;

    // no-GC buffers
    private readonly Vector3[] _corners = new Vector3[4];
    private readonly Vector3[] _verts   = new Vector3[8];

    // frame counter
    private int _frameCount = 0;

    // dirty check
    private Bounds _lastBounds;

    // set once
    private static readonly int[] _triangles = new int[]
    {
        // front
        0,1,2,  2,3,0,
        // back
        6,5,4,  4,7,6,
        // sides
        0,4,5,  5,1,0,
        1,5,6,  6,2,1,
        2,6,7,  7,3,2,
        3,7,4,  4,0,3
    };

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // cache light
        if (lightTransform != null)
            _lightComp = lightTransform.GetComponent<Light>();

        shadowMesh = new Mesh { name = "ShadowMesh" };
        shadowMesh.MarkDynamic(); // frequent updates

        // init mesh
        shadowMesh.vertices  = _verts;
        shadowMesh.triangles = _triangles;

        // MeshFilter
        MeshFilter mf = shadowObject.GetComponent<MeshFilter>();
        if (!mf) mf = shadowObject.gameObject.AddComponent<MeshFilter>();
        mf.mesh = shadowMesh;

        // MeshRenderer
        MeshRenderer mr = shadowObject.GetComponent<MeshRenderer>();
        if (!mr) mr = shadowObject.gameObject.AddComponent<MeshRenderer>();

        if (mr.sharedMaterial == null)
        {
            mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"))
            {
                color = new Color(0, 0, 0, 0.5f)
            };
        }

        // hide shadow until light is on
        shadowObject.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        _frameCount++;

        bool lit = IsLitBySpotLight();
        shadowObject.gameObject.SetActive(lit);

        if (!lit) return;

        // lazy create collider on first lit
        if (shadowCollider == null)
            CreateCollider();

        shadowCollider.enabled = true;

        // throttled mesh
        if (_frameCount % meshUpdateInterval == 0)
            UpdateShadow();

        // throttled collider
        if (_frameCount % colliderUpdateInterval == 0)
            UpdateBoxCollider();
    }

    // create collider only when first needed
    void CreateCollider()
    {
        shadowCollider = shadowObject.gameObject.AddComponent<BoxCollider>();
        shadowCollider.isTrigger = false;
    }

    void UpdateShadow()
    {
        GetSpriteCornersWorld(); // fill _corners

        // project to wall
        for (int i = 0; i < 4; i++)
            _verts[i] = ProjectToWall(_corners[i]);

        // depth verts
        Vector3 normal   = (_verts[1] - _verts[0]).normalized;
        Vector3 depthDir = Vector3.Cross(normal, Vector3.up).normalized * shadowThickness;

        for (int i = 0; i < 4; i++)
            _verts[i + 4] = _verts[i] - depthDir;

        // verts only, triangles unchanged
        shadowMesh.vertices = _verts;
        shadowMesh.RecalculateBounds();
        // no normals needed
    }

    // fit box to mesh bounds, skip if unchanged
    void UpdateBoxCollider()
    {
        Bounds b = shadowMesh.bounds;
        if (b == _lastBounds) return; // dirty check
        _lastBounds = b;
        shadowCollider.center = b.center;
        shadowCollider.size   = b.size;
    }

    // no-GC corners
    void GetSpriteCornersWorld()
    {
        Bounds b = sr.bounds;
        float cx = b.center.z;

        _corners[0] = new Vector3(b.min.x, b.min.y, cx);
        _corners[1] = new Vector3(b.max.x, b.min.y, cx);
        _corners[2] = new Vector3(b.max.x, b.max.y, cx);
        _corners[3] = new Vector3(b.min.x, b.max.y, cx);
    }

    Vector3 ProjectToWall(Vector3 from)
    {
        Vector3 dir = (from - lightTransform.position).normalized;

        if (Physics.Raycast(lightTransform.position, dir, out RaycastHit hit, 50f, wallMask))
        {
            Vector3 expandedPoint = hit.point + dir * shadowExpand;
            return shadowObject.InverseTransformPoint(expandedPoint);
        }

        return shadowObject.InverseTransformPoint(from);
    }

    bool IsLitBySpotLight()
    {
        // cached ref
        if (_lightComp == null || _lightComp.type != LightType.Spot || !lightTransform.gameObject.activeInHierarchy)
            return false;

        float dist = Vector3.Distance(transform.position, lightTransform.position);
        if (dist > _lightComp.range)
            return false;

        // angle check
        Vector3 dirToObject = (transform.position - lightTransform.position).normalized;
        float angle = Vector3.Angle(lightTransform.forward, dirToObject);

        return angle <= _lightComp.spotAngle * 0.5f;
    }
}