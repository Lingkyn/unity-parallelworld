using System.Collections.Generic;
using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// Unity 壳：挂载于常驻对象（如 GameplaySetup），驱动光圈并控制 Circle 的 Transform / 范围检测
    /// 需指定 apertureTarget（Circle）以支持 X 键关闭后再次开启
    /// </summary>
    public class ApertureController : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField, Tooltip("光圈视觉对象（含 SpriteRenderer 的 Circle），为空则使用本物体")]
        private Transform apertureTarget;
        [SerializeField] private Camera cam;
        [SerializeField] private LightInputAdapter lightInputAdapter;
        [SerializeField] private ApertureConfig config;
        [SerializeField, Tooltip("仅影响此层的对象")]
        private LayerMask affectedLayerMask;
        [SerializeField, Tooltip("是否始终面向相机")]
        private bool lookAtCamera = true;
        [SerializeField, Tooltip("勾选后在 Console 输出检测调试信息")]
        private bool debugLog;

        private ApertureCore _apertureCore;
        private Transform _target;
        private readonly List<GameObject> _inLight = new List<GameObject>();
        private string[] _lastInRangeNames = System.Array.Empty<string>();
        private bool _initialized;

        private Transform Target => _target != null ? _target : transform;

        private static void SetMeshRenderersEnabled(GameObject go, bool enabled)
        {
            foreach (var mr in go.GetComponentsInChildren<MeshRenderer>(true))
                mr.enabled = enabled;
        }

        private static void SetCollidersEnabled(GameObject go, bool enabled)
        {
            foreach (var col in go.GetComponentsInChildren<Collider>(true))
                col.enabled = enabled;
        }

        private static bool SetChanged(string[] prev, GameObject[] curr, out string[] names)
        {
            var currNames = System.Array.ConvertAll(curr, g => g != null ? g.name : "");
            System.Array.Sort(currNames);
            names = currNames;
            if (prev.Length != currNames.Length) return true;
            for (int i = 0; i < prev.Length; i++)
                if (prev[i] != currNames[i]) return true;
            return false;
        }

        private void Awake()
        {
            _target = apertureTarget != null ? apertureTarget : transform;

            if (cam == null) cam = Camera.main;
            if (affectedLayerMask == 0)
                affectedLayerMask = LayerMask.GetMask(Layers.ApertureAffected);
            if (cam == null) Debug.LogWarning("[ApertureController] 未找到主相机");

            if (lightInputAdapter == null)
                lightInputAdapter = GetComponent<LightInputAdapter>();
            if (lightInputAdapter == null)
                lightInputAdapter = FindAnyObjectByType<LightInputAdapter>();
            if (lightInputAdapter == null)
                Debug.LogWarning("[ApertureController] 未找到 LightInputAdapter");

            _apertureCore = new ApertureCore();

            if (config == null)
                Debug.LogWarning("[ApertureController] ApertureConfig 未分配，将使用默认值");

            float initialScale = Target.localScale.x;
            bool defaultActive = config != null ? config.defaultActive : true;
            _apertureCore.Init(initialScale, defaultActive);

            Target.gameObject.SetActive(defaultActive);
        }

        private void Update()
        {
            if (lightInputAdapter == null || cam == null) return;

            // 切换
            if (lightInputAdapter.GetTogglePressed())
            {
                _apertureCore.Toggle();
                Target.gameObject.SetActive(_apertureCore.IsActive);
                if (!_apertureCore.IsActive)
                {
                    RestoreAll();
                    _inLight.Clear();
                }
                return;
            }

            if (!_apertureCore.IsActive) return;

            // 跟随鼠标：射线与固定距离平面求交
            float distance = config != null ? config.planeDistance : 10f;
            var plane = new Plane(cam.transform.forward, cam.transform.position + cam.transform.forward * distance);

            Ray ray = cam.ScreenPointToRay(lightInputAdapter.GetMousePosition());
            if (plane.Raycast(ray, out float enter))
            {
                Target.position = ray.GetPoint(enter);
            }

            // 面向相机
            if (lookAtCamera && cam != null)
            {
                Target.rotation = Quaternion.LookRotation(Target.position - cam.transform.position);
            }

            // 缩放
            float scrollDelta = lightInputAdapter.GetScrollDelta();
            float minScale = config != null ? config.minScale : 1f;
            float maxScale = config != null ? config.maxScale : 10f;
            float scaleSpeed = config != null ? config.scaleSpeed : 2f;

            _apertureCore.UpdateScale(scrollDelta, scaleSpeed, minScale, maxScale);
            float scale = _apertureCore.Scale;
            Target.localScale = new Vector3(scale, scale, scale);
        }

        private void LateUpdate()
        {
            if (!_apertureCore.IsActive || !Target.gameObject.activeInHierarchy) return;

            if (!_initialized)
            {
                _initialized = true;
                foreach (var mr in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (((1 << mr.gameObject.layer) & affectedLayerMask) != 0)
                        mr.enabled = false;
                }
                foreach (var col in FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (((1 << col.gameObject.layer) & affectedLayerMask) != 0)
                        col.enabled = false;
                }
            }

            Vector3 center = Target.position;
            float radius = _apertureCore.Scale * 0.5f;
            RangeDetector.GetApertureBounds(center, radius, out float minX, out float maxX, out float minY, out float maxY);

            var inRange = RangeDetector.GetObjectsInApertureBounds(minX, maxX, minY, maxY, affectedLayerMask);

            if (debugLog && SetChanged(_lastInRangeNames, inRange, out _lastInRangeNames))
                Debug.Log($"[Aperture] 检测变化: {string.Join(", ", _lastInRangeNames)}");

            // 范围内（光照内）→ 开启 MeshRenderer 和 Collider
            foreach (var go in inRange)
            {
                if (go == null || go == Target.gameObject) continue;
                SetMeshRenderersEnabled(go, true);
                SetCollidersEnabled(go, true);
                if (!_inLight.Contains(go))
                    _inLight.Add(go);
            }

            // 光照内的对象：XY 超出范围则关闭 MeshRenderer 和 Collider
            for (int i = _inLight.Count - 1; i >= 0; i--)
            {
                var go = _inLight[i];
                if (go == null) { _inLight.RemoveAt(i); continue; }
                Vector3 p = go.transform.position;
                if (p.x < minX || p.x > maxX || p.y < minY || p.y > maxY)
                {
                    SetMeshRenderersEnabled(go, false);
                    SetCollidersEnabled(go, false);
                    _inLight.RemoveAt(i);
                }
            }
        }

        private void RestoreAll()
        {
            foreach (var go in _inLight)
            {
                if (go != null)
                {
                    SetMeshRenderersEnabled(go, false);
                    SetCollidersEnabled(go, false);
                }
            }
            _inLight.Clear();
        }

        private void OnDisable()
        {
            RestoreAll();
        }
    }
}
