using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteShadowSolid : MonoBehaviour
{
    [Header("Refs")]
    public Transform lightTransform;
    public Transform shadowObject;
    public LayerMask wallMask;

    [Header("Shadow Settings")]
    public float shadowExpand = 0.8f;
    public float shadowThickness = 0.8f; 

    private SpriteRenderer sr;
    private Mesh shadowMesh;
    private MeshCollider shadowCollider;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        shadowMesh = new Mesh();

        // ===== Collider =====
        shadowCollider = shadowObject.gameObject.AddComponent<MeshCollider>();
        shadowCollider.convex = true;
        shadowCollider.isTrigger = false; // 

        // ===== MeshFilter =====
        MeshFilter mf = shadowObject.GetComponent<MeshFilter>();
        if (!mf) mf = shadowObject.gameObject.AddComponent<MeshFilter>();

        // ===== Renderer =====
        MeshRenderer mr = shadowObject.GetComponent<MeshRenderer>();
        if (!mr) mr = shadowObject.gameObject.AddComponent<MeshRenderer>();

        mf.mesh = shadowMesh;

        if (mr.sharedMaterial == null)
        {
            mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            mr.sharedMaterial.color = new Color(0, 0, 0, 0.5f);
        }
    }

    void LateUpdate()
    {
        bool lit = IsLitBySpotLight();
        shadowObject.gameObject.SetActive(lit);

    
        if (shadowCollider != null)
           shadowCollider.enabled = lit;

        if (!lit) return;

        UpdateShadow();
    }

    void UpdateShadow()
    {
        Vector3[] spriteCorners = GetSpriteCornersWorld();

        // shadow on wall
        Vector3[] front = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            front[i] = ProjectToWall(spriteCorners[i]);
        }

        // thickness
        Vector3 normal = (front[1] - front[0]).normalized;
        Vector3 depthDir = Vector3.Cross(normal, Vector3.up).normalized * shadowThickness;

        Vector3[] verts = new Vector3[8];

        // exclude
        for (int i = 0; i < 4; i++)
            verts[i] = front[i];

        // 
        for (int i = 0; i < 4; i++)
            verts[i + 4] = front[i] - depthDir;

        shadowMesh.Clear();
        shadowMesh.vertices = verts;

        shadowMesh.triangles = new int[]
        {
            // front
            0,1,2, 2,3,0,
            // back
            6,5,4, 4,7,6,
            // sides
            0,4,5, 5,1,0,
            1,5,6, 6,2,1,
            2,6,7, 7,3,2,
            3,7,4, 4,0,3
        };

        shadowMesh.RecalculateBounds();
        shadowMesh.RecalculateNormals();

        shadowCollider.sharedMesh = shadowMesh;
    }

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

    Vector3 ProjectToWall(Vector3 from)
    {
        Vector3 dir = (from - lightTransform.position).normalized;

        RaycastHit hit;

        if (Physics.Raycast(lightTransform.position, dir, out hit, 50f, wallMask))
        {
            Vector3 expandedPoint = hit.point + dir * shadowExpand;
            return shadowObject.InverseTransformPoint(expandedPoint);
        }

        return shadowObject.InverseTransformPoint(from);
    }

    bool IsLitBySpotLight()
    {
        Light lightComp = lightTransform.GetComponent<Light>();
        if (lightComp == null || lightComp.type != LightType.Spot)
            return true; 

        
        float dist = Vector3.Distance(transform.position, lightTransform.position);
        if (dist > lightComp.range)
            return false;

    // ===== 角度判断 =====
        Vector3 dirToObject = (transform.position - lightTransform.position).normalized;
        float angle = Vector3.Angle(lightTransform.forward, dirToObject);

        if (angle > lightComp.spotAngle * 0.5f)
            return false;

        return true;
    }
}