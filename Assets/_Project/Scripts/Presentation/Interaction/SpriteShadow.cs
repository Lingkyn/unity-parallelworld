using UnityEngine;
using System.Collections.Generic;
using ParallelWorld;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteShadow : MonoBehaviour
{
    [System.Serializable]
    public class LightShadowEntry
    {
        public Transform lightTransform;
        public Transform shadowObject;

        [HideInInspector] public Light lightComp;
        [HideInInspector] public Mesh shadowMesh;
        [HideInInspector] public BoxCollider shadowCollider;
        [HideInInspector] public Rigidbody shadowBody;
        [HideInInspector] public Bounds lastBounds;
        [HideInInspector] public bool initialized;

        public readonly Vector3[] verts = new Vector3[8];
    }

    [Header("Config")]
    [Tooltip("可选，配置阴影与性能参数；未分配时使用下方默认值")]
    public SpriteShadowConfig config;

    [Header("Refs")]
    [Tooltip("多灯源配置（推荐使用）")]
    public List<LightShadowEntry> lightEntries = new List<LightShadowEntry>();

    [Header("Legacy Refs (兼容旧场景)")]
    public Transform lightTransform;
    public Transform shadowObject;
    public LayerMask wallMask;

    [Header("Shadow Settings (config 未分配时生效)")]
    public float shadowExpand = -0.2f;
    public float shadowThickness = 0.6f;

    [Header("Performance (config 未分配时生效)")]
    public int meshUpdateInterval = 2;
    public int colliderUpdateInterval = 6;

    [Header("Ground Stability")]
    [Tooltip("给阴影平台顶部额外增加的支撑高度，减少角色后段掉落")]
    public float topSupportPadding = 0.12f;

    private float ShadowExpand => config != null ? config.shadowExpand : shadowExpand;
    private float ShadowThickness => config != null ? config.shadowThickness : shadowThickness;
    private int MeshUpdateInterval => config != null ? config.meshUpdateInterval : meshUpdateInterval;
    private int ColliderUpdateInterval => config != null ? config.colliderUpdateInterval : colliderUpdateInterval;
    private float ProjectionRayDistance => config != null ? Mathf.Max(10f, config.projectionRayDistance) : 200f;

    private const string DEFAULT_LAYER_NAME = "Default";
    private const string STENCIL_LAYER_NAME = "RealObjects";

    private SpriteRenderer sr;
    private readonly Vector3[] _corners = new Vector3[4];
    private int _frameCount = 0;
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

        if (config != null)
        {
            wallMask = wallMask.value == 0 ? wallMask : wallMask;
        }

        _defaultLayer = LayerMask.NameToLayer(DEFAULT_LAYER_NAME);
        _stencilLayer = LayerMask.NameToLayer(STENCIL_LAYER_NAME);

        if (_defaultLayer < 0)
            _defaultLayer = gameObject.layer;

        if (_stencilLayer < 0)
            _stencilLayer = _defaultLayer;

        BuildEntriesIfNeeded();
        AutoSetupEntriesFromConfig();
        InitializeEntries();
    }

    void LateUpdate()
    {
        _frameCount++;

        bool anyLit = false;

        for (int i = 0; i < lightEntries.Count; i++)
        {
            LightShadowEntry entry = lightEntries[i];
            if (!entry.initialized)
                continue;

            bool lit = IsLitBySpotLight(entry);
            anyLit |= lit;
            entry.shadowObject.gameObject.SetActive(lit);

            if (!lit)
            {
                if (entry.shadowCollider != null)
                    entry.shadowCollider.enabled = false;
                continue;
            }

            if (entry.shadowCollider == null)
                CreateCollider(entry);

            entry.shadowCollider.enabled = true;

            if (_frameCount % Mathf.Max(1, MeshUpdateInterval) == 0)
                UpdateShadow(entry);

            // Keep collider in sync every frame for stable grounding on moving shadow platforms.
            if (_frameCount % Mathf.Max(1, ColliderUpdateInterval) == 0 || _frameCount % Mathf.Max(1, MeshUpdateInterval) == 0)
                UpdateBoxCollider(entry);
        }

        if (anyLit != _lastLitState)
        {
            int targetLayer = anyLit ? _stencilLayer : _defaultLayer;
            SetLayerRecursively(transform, targetLayer);
            _lastLitState = anyLit;
        }
    }

    void BuildEntriesIfNeeded()
    {
        // 兼容旧场景：如果列表为空，则自动把旧字段组装成一个 entry。
        if (lightEntries != null && lightEntries.Count > 0)
            return;

        lightEntries = new List<LightShadowEntry>();

        if (lightTransform == null || shadowObject == null)
            return;

        lightEntries.Add(new LightShadowEntry
        {
            lightTransform = lightTransform,
            shadowObject = shadowObject
        });
    }

    void AutoSetupEntriesFromConfig()
    {
        if (config == null)
            return;

        if ((lightEntries == null || lightEntries.Count == 0) && config.autoCollectSpotLightsInChildren)
        {
            Light[] lights = GetComponentsInChildren<Light>(true);
            lightEntries = new List<LightShadowEntry>();

            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] == null || lights[i].type != LightType.Spot)
                    continue;

                lightEntries.Add(new LightShadowEntry
                {
                    lightTransform = lights[i].transform
                });
            }
        }

        if (lightEntries == null)
            lightEntries = new List<LightShadowEntry>();

        for (int i = 0; i < lightEntries.Count; i++)
        {
            LightShadowEntry entry = lightEntries[i];
            if (entry == null || entry.lightTransform == null || entry.shadowObject != null)
                continue;

            entry.shadowObject = FindOrCreateShadowObject(entry.lightTransform);
        }
    }

    Transform FindOrCreateShadowObject(Transform light)
    {
        string shadowName = BuildShadowObjectName(light);
        Transform root = ResolveShadowRoot();
        Transform existing = root.Find(shadowName);
        if (existing != null)
            return existing;

        if (config == null || !config.autoCreateShadowObject)
            return null;

        GameObject go = new GameObject(shadowName);
        go.transform.SetParent(root, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.SetActive(false);
        return go.transform;
    }

    Transform ResolveShadowRoot()
    {
        if (config == null || string.IsNullOrWhiteSpace(config.shadowRootName))
            return transform;

        Transform root = transform.Find(config.shadowRootName);
        if (root != null)
            return root;

        if (!config.autoCreateShadowObject)
            return transform;

        GameObject rootObj = new GameObject(config.shadowRootName);
        rootObj.transform.SetParent(transform, false);
        rootObj.transform.localPosition = Vector3.zero;
        rootObj.transform.localRotation = Quaternion.identity;
        rootObj.transform.localScale = Vector3.one;
        return rootObj.transform;
    }

    string BuildShadowObjectName(Transform light)
    {
        string prefix = (config != null && !string.IsNullOrWhiteSpace(config.shadowObjectPrefix))
            ? config.shadowObjectPrefix
            : "Shadow_";

        return prefix + light.name;
    }

    void InitializeEntries()
    {
        for (int i = 0; i < lightEntries.Count; i++)
        {
            LightShadowEntry entry = lightEntries[i];
            if (entry == null || entry.lightTransform == null || entry.shadowObject == null)
                continue;

            entry.lightComp = entry.lightTransform.GetComponent<Light>();

            entry.shadowMesh = new Mesh { name = "ShadowMesh_" + i };
            entry.shadowMesh.MarkDynamic();
            entry.shadowMesh.vertices = entry.verts;
            entry.shadowMesh.triangles = _triangles;

            MeshFilter mf = entry.shadowObject.GetComponent<MeshFilter>();
            if (!mf) mf = entry.shadowObject.gameObject.AddComponent<MeshFilter>();
            mf.mesh = entry.shadowMesh;

            MeshRenderer mr = entry.shadowObject.GetComponent<MeshRenderer>();
            if (!mr) mr = entry.shadowObject.gameObject.AddComponent<MeshRenderer>();

            if (config != null && config.shadowMaterial != null)
            {
                mr.sharedMaterial = config.shadowMaterial;
            }
            else if (mr.sharedMaterial == null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"))
                {
                    color = config != null ? config.shadowColor : new Color(0, 0, 0, 0.5f)
                };
            }
            else if (config != null)
            {
                mr.sharedMaterial.color = config.shadowColor;
            }

            entry.shadowCollider = entry.shadowObject.GetComponent<BoxCollider>();
            if (entry.shadowCollider != null)
                entry.shadowCollider.isTrigger = config != null ? config.shadowColliderIsTrigger : false;

            EnsureKinematicBody(entry);

            entry.shadowObject.gameObject.SetActive(false);
            entry.initialized = true;
        }
    }

    void CreateCollider(LightShadowEntry entry)
    {
        entry.shadowCollider = entry.shadowObject.gameObject.AddComponent<BoxCollider>();
        entry.shadowCollider.isTrigger = config != null ? config.shadowColliderIsTrigger : false;
        EnsureKinematicBody(entry);
    }

    void EnsureKinematicBody(LightShadowEntry entry)
    {
        entry.shadowBody = entry.shadowObject.GetComponent<Rigidbody>();
        if (entry.shadowBody == null)
            entry.shadowBody = entry.shadowObject.gameObject.AddComponent<Rigidbody>();

        entry.shadowBody.isKinematic = true;
        entry.shadowBody.useGravity = false;
        entry.shadowBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        entry.shadowBody.interpolation = RigidbodyInterpolation.Interpolate;
        entry.shadowBody.constraints = RigidbodyConstraints.FreezeAll;
    }

    void UpdateShadow(LightShadowEntry entry)
    {
        GetSpriteCornersWorld();

        bool hasFallbackPlane = TryGetFallbackWallPlane(entry, out Plane fallbackPlane);

        for (int i = 0; i < 4; i++)
            entry.verts[i] = ProjectToWall(_corners[i], entry, hasFallbackPlane, fallbackPlane);

        Vector3 normal = (entry.verts[1] - entry.verts[0]).normalized;
        Vector3 depthDir = Vector3.Cross(normal, Vector3.up).normalized * ShadowThickness;

        for (int i = 0; i < 4; i++)
            entry.verts[i + 4] = entry.verts[i] - depthDir;

        entry.shadowMesh.vertices = entry.verts;
        entry.shadowMesh.RecalculateBounds();
    }

    void UpdateBoxCollider(LightShadowEntry entry)
    {
        Bounds b = entry.shadowMesh.bounds;

        if (topSupportPadding > 0f)
        {
            b.center += new Vector3(0f, topSupportPadding * 0.5f, 0f);
            b.size += new Vector3(0f, topSupportPadding, 0f);
        }

        if (b == entry.lastBounds) return;
        entry.lastBounds = b;
        entry.shadowCollider.center = b.center;
        entry.shadowCollider.size = b.size;
        Physics.SyncTransforms();
    }

    void GetSpriteCornersWorld()
    {
        if (sr.sprite == null)
        {
            Bounds b = sr.bounds;
            float cx = b.center.z;

            _corners[0] = new Vector3(b.min.x, b.min.y, cx);
            _corners[1] = new Vector3(b.max.x, b.min.y, cx);
            _corners[2] = new Vector3(b.max.x, b.max.y, cx);
            _corners[3] = new Vector3(b.min.x, b.max.y, cx);
            return;
        }

        Bounds localBounds = sr.sprite.bounds;

        Vector3 localBL = new Vector3(localBounds.min.x, localBounds.min.y, 0f);
        Vector3 localBR = new Vector3(localBounds.max.x, localBounds.min.y, 0f);
        Vector3 localTR = new Vector3(localBounds.max.x, localBounds.max.y, 0f);
        Vector3 localTL = new Vector3(localBounds.min.x, localBounds.max.y, 0f);

        if (sr.flipX)
        {
            localBL.x = -localBL.x;
            localBR.x = -localBR.x;
            localTR.x = -localTR.x;
            localTL.x = -localTL.x;
        }

        if (sr.flipY)
        {
            localBL.y = -localBL.y;
            localBR.y = -localBR.y;
            localTR.y = -localTR.y;
            localTL.y = -localTL.y;
        }

        _corners[0] = sr.transform.TransformPoint(localBL);
        _corners[1] = sr.transform.TransformPoint(localBR);
        _corners[2] = sr.transform.TransformPoint(localTR);
        _corners[3] = sr.transform.TransformPoint(localTL);
    }

    bool TryGetFallbackWallPlane(LightShadowEntry entry, out Plane plane)
    {
        Vector3 center = (_corners[0] + _corners[1] + _corners[2] + _corners[3]) * 0.25f;
        Vector3 rayOrigin = entry.lightTransform.position;
        Vector3 dir = (center - rayOrigin).normalized;

        if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, ProjectionRayDistance, wallMask, QueryTriggerInteraction.Ignore))
        {
            plane = new Plane(hit.normal, hit.point);
            return true;
        }

        plane = default;
        return false;
    }

    Vector3 ProjectToWall(Vector3 from, LightShadowEntry entry, bool hasFallbackPlane, Plane fallbackPlane)
    {
        if (entry.lightComp == null)
            return entry.shadowObject.InverseTransformPoint(from);

        Vector3 rayOrigin = entry.lightTransform.position;
        Vector3 dir = (from - rayOrigin).normalized;

        if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, ProjectionRayDistance, wallMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 expandedPoint = hit.point + dir * ShadowExpand;
            return entry.shadowObject.InverseTransformPoint(expandedPoint);
        }

        if (hasFallbackPlane)
        {
            Ray ray = new Ray(rayOrigin, dir);
            if (fallbackPlane.Raycast(ray, out float enter))
            {
                Vector3 fallbackPoint = ray.GetPoint(enter) + dir * ShadowExpand;
                return entry.shadowObject.InverseTransformPoint(fallbackPoint);
            }
        }

        return entry.shadowObject.InverseTransformPoint(from);
    }

    bool IsLitBySpotLight(LightShadowEntry entry)
    {
        if (entry.lightComp == null || entry.lightComp.type != LightType.Spot || !entry.lightTransform.gameObject.activeInHierarchy || !entry.lightComp.enabled)
            return false;

        float dist = Vector3.Distance(transform.position, entry.lightTransform.position);
        if (dist > entry.lightComp.range)
            return false;

        Vector3 dirToObject = (transform.position - entry.lightTransform.position).normalized;
        float angle = Vector3.Angle(entry.lightTransform.forward, dirToObject);

        return angle <= entry.lightComp.spotAngle * 0.5f;
    }

    void SetLayerRecursively(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform child in t)
            SetLayerRecursively(child, layer);
    }
}