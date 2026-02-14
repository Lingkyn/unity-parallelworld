using UnityEditor;
using UnityEngine;

namespace ParallelWorld.Editor
{
    /// <summary>
    /// 统一创建默认配置资源
    /// Create required ScriptableObject configs in one click.
    /// Skip if asset exists, avoid overwriting.
    /// Output to configured folder path.
    /// </summary>
    public static class CreateDefaultConfigs
    {
        private const string ConfigOutputPath = "Assets/_Project/Data";

        [MenuItem("Tools/Create Default Configs")]
        public static void Execute()
        {
            EnsureDirectory(ConfigOutputPath);
            int created = 0;
            int skipped = 0;

            // MovementConfig
            if (TryCreateConfig<MovementConfig>("MovementConfig.asset", out bool createdMovement))
            {
                if (createdMovement) created++; else skipped++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CreateDefaultConfigs] 完成: 新建 {created} 个, 跳过(已存在) {skipped} 个");
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder("Assets"))
                return;

            string[] parts = path.Replace("\\", "/").Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    string parent = current;
                    AssetDatabase.CreateFolder(parent, parts[i]);
                }
                current = next;
            }
        }

        private static bool TryCreateConfig<T>(string fileName, out bool wasCreated) where T : ScriptableObject
        {
            wasCreated = false;
            string fullPath = ConfigOutputPath + "/" + fileName;

            if (AssetDatabase.LoadAssetAtPath<T>(fullPath) != null)
            {
                return true;
            }

            T instance = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(instance, fullPath);
            wasCreated = true;
            return true;
        }
    }
}
