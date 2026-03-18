using UnityEngine;
using ParallelWorld;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteShadow : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("可选，配置阴影与性能参数；未分配时使用下方默认值")]
    public SpriteShadowConfig config;

    [Header("Refs")]
    public Transform lightTransform;
    public Transform shadowObject;
    public LayerMask wallMask;

    [Header("Shadow Settings (config 未分配时生效)")]
    public float shadowExpand = -0.2f;
    public float shadowThickness = 0.6f;

    [Header("Performance (config 未分配时生效)")]
    public int meshUpdateInterval = 2;
    public int colliderUpdateInterval = 6;

    private float ShadowExpand => config != null ? config.shadowExpand : shadowExpand;
    private float ShadowThickness => config != null ? config.shadowThickness : shadowThickness;
    private int MeshUpdateInterval => config != null ? config.meshUpdateInterval : meshUpdateInterval;
    private int ColliderUpdateInterval => config != null ? config.colliderUpdateInterval : colliderUpdateInterval;

    private const string DEFAULT_LAYER_NAME = "Default";
    private const string STENCIL_LAYER_NAME = "RealObjects";

    private SpriteRenderer sr;
    private Mesh shadowMesh;
    private BoxCollider shadowCollider;
    private Light _lightComp;
    private readonly Vector3[] _corners = new Vector3[4];
    private readonly Vector3[] _verts = new Vector3[8];
    private int _frameCount = 0;
    private Bounds _lastBounds;
    private int _defaultLayer;
    private int _stencilLayer;
    private bool _lastLitState = false;

    private static readonly int[] _triangles = new int[]
    {
        0,1,2, 2,3,0,
        6,5,4, 4,7,6,
        0,4,5, 5,1,0,
        1,5,6, 6,2,1,
        2,6,7, 7,3,2,
        3,7,4, 4,0,3
    };

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (lightTransform != null)
            _lightComp = lightTransform.GetComponent<Light>();

        shadowMesh = new Mesh { name = "ShadowMesh" };
        shadowMesh.MarkDynamic();
        shadowMesh.vertices = _verts;
        shadowMesh.triangles = _triangles;

        MeshFilter mf = shadowObject.GetComponent<MeshFilter>();
        if (!mf) mf = shadowObject.gameObject.AddComponent<MeshFilter>();
        mf.mesh = shadowMesh;

        MeshRenderer mr = shadowObject.GetComponent<MeshRenderer>();
        if (!mr) mr = shadowObject.gameObject.AddComponent<MeshRenderer>();

        if (mr.sharedMaterial == null)
        {
            mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"))
            {
                color = new Color(0, 0, 0, 0.5f)
            };
        }

        shadowObject.gameObject.SetActive(false);

        _defaultLayer = LayerMask.NameToLayer(DEFAULT_LAYER_NAME);
        _stencilLayer = LayerMask.NameToLayer(STENCIL_LAYER_NAME);
    }

    void LateUpdate()
    {
        _frameCount++;

        bool lit = IsLitBySpotLight();

        if (lit != _lastLitState)
        {
            int targetLayer = lit ? _stencilLayer : _defaultLayer;
            SetLayerRecursively(transform, targetLayer);
            _lastLitState = lit;
        }

        shadowObject.gameObject.SetActive(lit);

        if (!lit) return;

        if (shadowCollider == null)
            CreateCollider();

        shadowCollider.enabled = true;

        if (_frameCount % MeshUpdateInterval == 0)
            UpdateShadow();

        if (_frameCount % ColliderUpdateInterval == 0)
            UpdateBoxCollider();
    }

    void CreateCollider()
    {
        shadowCollider = shadowObject.gameObject.AddComponent<BoxCollider>();
        shadowCollider.isTrigger = false;
    }

    void UpdateShadow()
    {
        GetSpriteCornersWorld();

        for (int i = 0; i < 4; i++)
            _verts[i] = ProjectToWall(_corners[i]);

        Vector3 normal = (_verts[1] - _verts[0]).normalized;
        Vector3 depthDir = Vector3.Cross(normal, Vector3.up).normalized * ShadowThickness;

        for (int i = 0; i < 4; i++)
            _verts[i + 4] = _verts[i] - depthDir;

        shadowMesh.vertices = _verts;
        shadowMesh.RecalculateBounds();
    }

    void UpdateBoxCollider()
    {
        Bounds b = shadowMesh.bounds;
        if (b == _lastBounds) return;
        _lastBounds = b;
        shadowCollider.center = b.center;
        shadowCollider.size = b.size;
    }

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
            Vector3 expandedPoint = hit.point + dir * ShadowExpand;
            return shadowObject.InverseTransformPoint(expandedPoint);
        }

        return shadowObject.InverseTransformPoint(from);
    }

    bool IsLitBySpotLight()
    {
        if (_lightComp == null || _lightComp.type != LightType.Spot || !lightTransform.gameObject.activeInHierarchy)
            return false;

        float dist = Vector3.Distance(transform.position, lightTransform.position);
        if (dist > _lightComp.range)
            return false;

        Vector3 dirToObject = (transform.position - lightTransform.position).normalized;
        float angle = Vector3.Angle(lightTransform.forward, dirToObject);

        return angle <= _lightComp.spotAngle * 0.5f;
    }

    void SetLayerRecursively(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform child in t)
            SetLayerRecursively(child, layer);
    }
}