using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ParallelWorld.Editor
{
    /// <summary>
    /// 统一查找缺失引用
    /// Find missing refs in scenes and prefabs.
    /// Batch scan and report paths &amp; GameObjects.
    /// Help debug broken references.
    /// </summary>
    public static class FindMissingReferences
    {
        private struct MissingRefEntry
        {
            public string AssetPath;
            public string GameObjectPath;
            public string ComponentOrField;
        }

        [MenuItem("Tools/Find Missing References/In Current Scene")]
        public static void FindInCurrentScene()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.isLoaded)
            {
                Debug.LogWarning("[FindMissingReferences] 当前无已加载场景");
                return;
            }
            FindInScenes(new[] { scene.path });
        }

        [MenuItem("Tools/Find Missing References/In All Scenes")]
        public static void FindInAllScenes()
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project" });
            var paths = new List<string>();
            foreach (string g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (!string.IsNullOrEmpty(p)) paths.Add(p);
            }
            if (paths.Count == 0)
            {
                Debug.Log("[FindMissingReferences] 未找到场景");
                return;
            }
            FindInScenes(paths.ToArray());
        }

        [MenuItem("Tools/Find Missing References/In All Prefabs")]
        public static void FindInAllPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project" });
            var paths = new List<string>();
            foreach (string g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (!string.IsNullOrEmpty(p)) paths.Add(p);
            }
            if (paths.Count == 0)
            {
                Debug.Log("[FindMissingReferences] 未找到 Prefab");
                return;
            }
            FindInPrefabs(paths.ToArray());
        }

        [MenuItem("Tools/Find Missing References/In All Scenes And Prefabs")]
        public static void FindInAll()
        {
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project" });
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project" });

            var scenePaths = new List<string>();
            foreach (string g in sceneGuids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (!string.IsNullOrEmpty(p)) scenePaths.Add(p);
            }

            var prefabPaths = new List<string>();
            foreach (string g in prefabGuids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (!string.IsNullOrEmpty(p)) prefabPaths.Add(p);
            }

            var results = new List<MissingRefEntry>();
            ScanScenes(scenePaths.ToArray(), results);
            ScanPrefabs(prefabPaths.ToArray(), results);
            ReportResults(results, "场景与 Prefab");
        }

        private static void FindInScenes(string[] paths)
        {
            var results = new List<MissingRefEntry>();
            ScanScenes(paths, results);
            ReportResults(results, "场景");
        }

        private static void FindInPrefabs(string[] paths)
        {
            var results = new List<MissingRefEntry>();
            ScanPrefabs(paths, results);
            ReportResults(results, "Prefab");
        }

        private static void ScanScenes(string[] paths, List<MissingRefEntry> results)
        {
            string activePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            foreach (string path in paths)
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                foreach (var root in scene.GetRootGameObjects())
                {
                    ScanGameObject(root, path, root.name, results);
                }
            }
            if (!string.IsNullOrEmpty(activePath))
                EditorSceneManager.OpenScene(activePath, OpenSceneMode.Single);
        }

        private static void ScanPrefabs(string[] paths, List<MissingRefEntry> results)
        {
            foreach (string path in paths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                ScanGameObject(prefab, path, prefab.name, results);
            }
        }

        private static void ScanGameObject(GameObject go, string assetPath, string rootName, List<MissingRefEntry> results)
        {
            string GetPath(Transform t)
            {
                var chain = new List<string>();
                while (t != null)
                {
                    chain.Insert(0, t.name);
                    t = t.parent;
                }
                return string.Join("/", chain);
            }

            foreach (var t in go.GetComponentsInChildren<Transform>(true))
            {
                var components = t.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        results.Add(new MissingRefEntry
                        {
                            AssetPath = assetPath,
                            GameObjectPath = GetPath(t),
                            ComponentOrField = $"(Missing Script #{i})"
                        });
                        continue;
                    }

                    var so = new SerializedObject(components[i]);
                    var sp = so.GetIterator();
                    while (sp.Next(true))
                    {
                        if (sp.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
                            {
                                results.Add(new MissingRefEntry
                                {
                                    AssetPath = assetPath,
                                    GameObjectPath = GetPath(t),
                                    ComponentOrField = $"{components[i].GetType().Name}.{sp.name}"
                                });
                            }
                        }
                    }
                }
            }
        }

        private static void ReportResults(List<MissingRefEntry> results, string scope)
        {
            if (results.Count == 0)
            {
                Debug.Log($"[FindMissingReferences] {scope} 中未发现缺失引用");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[FindMissingReferences] 在 {scope} 中发现 {results.Count} 处缺失引用:");
            string currentPath = "";
            foreach (var r in results)
            {
                if (r.AssetPath != currentPath)
                {
                    currentPath = r.AssetPath;
                    sb.AppendLine($"  [{currentPath}]");
                }
                sb.AppendLine($"    - {r.GameObjectPath} -> {r.ComponentOrField}");
            }
            Debug.LogWarning(sb.ToString());
        }
    }
}
