using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class InteractiveShadow : MonoBehaviour
{
    [Header("Refs")]
    public Transform lightTransform;
    public Transform shadowObject;
    public LayerMask wallMask;

    [Header("Shadow Settings")]
    public float shadowExpand = 0.8f;
    public float shadowThickness = 0.8f;

    [Header("Performance")]
    [Range(0.02f, 0.5f)]
    public float updateInterval = 0.08f;

    // ===== cached =====
    private SpriteRenderer sr;
    private Light cachedLight;
    private Mesh shadowMesh;
    private MeshCollider shadowCollider;
    private MeshFilter shadowMF;

    // ===== transform cache =====
    private Vector3 lastPos;
    private Quaternion lastRot;
    private Vector3 lastScale;
    private bool canUpdate = true;

    // ===== static triangles（永不变化）=====
    private static readonly int[] tris = new int[]
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
        cachedLight = lightTransform ? lightTransform.GetComponent<Light>() : null;

        shadowMesh = new Mesh();
        shadowMesh.MarkDynamic(); // ⭐ 提示Unity这是动态mesh

        // ===== MeshFilter =====
        shadowMF = shadowObject.GetComponent<MeshFilter>();
        if (!shadowMF) shadowMF = shadowObject.gameObject.AddComponent<MeshFilter>();
        shadowMF.sharedMesh = shadowMesh;

        // ===== Renderer =====
        MeshRenderer mr = shadowObject.GetComponent<MeshRenderer>();
        if (!mr) mr = shadowObject.gameObject.AddComponent<MeshRenderer>();

        if (mr.sharedMaterial == null)
        {
            mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            mr.sharedMaterial.color = new Color(0, 0, 0, 0.5f);
        }

        // ===== Collider =====
        shadowCollider = shadowObject.gameObject.AddComponent<MeshCollider>();
        shadowCollider.convex = true;
        shadowCollider.isTrigger = false;

        // triangles 只设置一次 ⭐⭐⭐
        shadowMesh.triangles = tris;

        CacheTransform();
    }

    void LateUpdate()
    {
        bool lit = IsLitBySpotLight();

        if (shadowObject.gameObject.activeSelf != lit)
            shadowObject.gameObject.SetActive(lit);

        if (shadowCollider && shadowCollider.enabled != lit)
            shadowCollider.enabled = lit;

        if (!lit) return;

        if (TransformChanged() && canUpdate)
        {
            Invoke(nameof(UpdateShadow), updateInterval);
            canUpdate = false;
        }

        CacheTransform();
    }

    // =============================
    // 🔥 Shadow Update（核心）
    // =============================
    void UpdateShadow()
    {
        Vector3[] spriteCorners = GetSpriteCornersWorld();

        Vector3[] front = new Vector3[4];
        for (int i = 0; i < 4; i++)
            front[i] = ProjectToWall(spriteCorners[i]);

        // 厚度方向
        Vector3 normal = (front[1] - front[0]).normalized;
        Vector3 depthDir = Vector3.Cross(normal, Vector3.up).normalized * shadowThickness;

        Vector3[] verts = new Vector3[8];

        for (int i = 0; i < 4; i++)
            verts[i] = front[i];

        for (int i = 0; i < 4; i++)
            verts[i + 4] = front[i] - depthDir;

        // ⭐⭐⭐ 不再 Clear
        shadowMesh.vertices = verts;
        shadowMesh.RecalculateBounds();
        shadowMesh.RecalculateNormals();

        // ⭐⭐⭐ 安全刷新 collider
        shadowCollider.sharedMesh = null;
        shadowCollider.sharedMesh = shadowMesh;

        canUpdate = true;
    }

    // =============================
    // 📐 Sprite corners
    // =============================
    Vector3[] GetSpriteCornersWorld()
    {
        Bounds b = sr.bounds;

        return new Vector3[]
        {
            new Vector3(b.min.x, b.min.y, b.center.z),
            new Vector3(b.max.x, b.min.y, b.center.z),
            new Vector3(b.max.x, b.max.y, b.center.z),
            new Vector3(b.min.x, b.max.y, b.center.z),
        };
    }

    // =============================
    // 🎯 Projection
    // =============================
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

    // =============================
    // 💡 Light check（优化版）
    // =============================
    bool IsLitBySpotLight()
    {
        if (cachedLight == null || cachedLight.type != LightType.Spot)
            return true;

        Vector3 diff = transform.position - lightTransform.position;
        float sqrDist = diff.sqrMagnitude;

        float range = cachedLight.range;
        if (sqrDist > range * range)
            return false;

        Vector3 dirToObject = diff.normalized;
        float angle = Vector3.Angle(lightTransform.forward, dirToObject);

        return angle <= cachedLight.spotAngle * 0.5f;
    }

    // =============================
    // 🔍 Transform change detection
    // =============================
    bool TransformChanged()
    {
        return transform.position != lastPos ||
               transform.rotation != lastRot ||
               transform.localScale != lastScale;
    }

    void CacheTransform()
    {
        lastPos = transform.position;
        lastRot = transform.rotation;
        lastScale = transform.localScale;
    }
}