using ParallelWorld;
using UnityEditor;
using UnityEngine;

namespace ParallelWorld.Editor
{
    /// <summary>
    /// 一键设置死亡复活系统：创建 DeathRespawnConfig、添加 DeathZone Layer、
    /// 为 Player 添加 DeathRespawnController，为场景中的死亡区域/复活点挂载 Detector。
    /// 运行：Tools -> Setup Death Respawn
    /// </summary>
    public static class SetupDeathRespawn
    {
        private const string ConfigPath = "Assets/_Project/Data/DeathRespawnConfig.asset";

        [MenuItem("Tools/Setup Death Respawn")]
        public static void Execute()
        {
            EnsureDeathZoneLayer();
            EnsureDeathRespawnConfig();
            SetupPlayer();
            SetupDeathZonesAndRespawnPoints();
            Debug.Log("[SetupDeathRespawn] 完成。请确认：1) Project Settings → Physics 中 Player 与 DeathZone 可触发；2) 死亡区域物体 Layer=DeathZone、挂 DeathZoneDetector；3) 复活点物体 Tag=Respawn、挂 RespawnPointDetector");
        }

        /// <summary>
        /// 为 Layer=DeathZone 的物体添加 DeathZoneDetector 和 Trigger Collider；
        /// 名称含 DeathZone 的物体若未设置 Layer 则自动设为 DeathZone 并添加 Detector；
        /// 为 Tag=Respawn 的物体添加 RespawnPointDetector。
        /// </summary>
        private static void SetupDeathZonesAndRespawnPoints()
        {
            int deathZoneLayer = LayerMask.NameToLayer(Layers.DeathZone);
            if (deathZoneLayer < 0) return;

            var allRoots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            int deathZoneCount = 0, respawnCount = 0;

            foreach (var root in allRoots)
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    var go = t.gameObject;
                    bool isDeathZone = go.layer == deathZoneLayer ||
                        go.name.IndexOf("DeathZone", System.StringComparison.OrdinalIgnoreCase) >= 0;
                    if (isDeathZone)
                    {
                        if (go.GetComponent<DeathZoneDetector>() == null)
                        {
                            go.layer = deathZoneLayer;
                            go.AddComponent<DeathZoneDetector>();
                            var col = go.GetComponent<Collider>();
                            if (col != null && !col.isTrigger)
                                col.isTrigger = true;
                            else if (col == null)
                                go.AddComponent<BoxCollider>().isTrigger = true;
                            deathZoneCount++;
                        }
                    }
                    else if (go.CompareTag(Tags.Respawn))
                    {
                        if (go.GetComponent<RespawnPointDetector>() == null)
                        {
                            go.AddComponent<RespawnPointDetector>();
                            var col = go.GetComponent<Collider>();
                            if (col != null && !col.isTrigger)
                                col.isTrigger = true;
                            else if (col == null)
                                go.AddComponent<BoxCollider>().isTrigger = true;
                            respawnCount++;
                        }
                    }
                }
            }

            if (deathZoneCount > 0 || respawnCount > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                Debug.Log($"[SetupDeathRespawn] 已为 {deathZoneCount} 个死亡区域添加 DeathZoneDetector，为 {respawnCount} 个复活点添加 RespawnPointDetector");
            }
        }

        private static void EnsureDeathZoneLayer()
        {
            var tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagManager == null || tagManager.Length == 0) return;

            var so = new SerializedObject(tagManager[0]);
            var layers = so.FindProperty("layers");
            if (layers == null) return;

            int idx = -1;
            for (int i = 8; i < layers.arraySize; i++)
            {
                var elem = layers.GetArrayElementAtIndex(i);
                if (elem.stringValue == Layers.DeathZone)
                    return; // 已存在
                if (idx < 0 && string.IsNullOrEmpty(elem.stringValue))
                    idx = i;
            }

            if (idx >= 0)
            {
                layers.GetArrayElementAtIndex(idx).stringValue = Layers.DeathZone;
                so.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                Debug.Log($"[SetupDeathRespawn] 已添加 Layer: {Layers.DeathZone} (index {idx})");
            }
            else
            {
                Debug.LogWarning("[SetupDeathRespawn] 无空闲 Layer 槽位，请手动在 Project Settings → Tags and Layers 中添加 DeathZone");
            }
        }

        private static void EnsureDeathRespawnConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<DeathRespawnConfig>(ConfigPath);
            if (config == null)
            {
                CreateDefaultConfigs.Execute();
                config = AssetDatabase.LoadAssetAtPath<DeathRespawnConfig>(ConfigPath);
            }

            if (config != null)
            {
                var so = new SerializedObject(config);
                var layerProp = so.FindProperty("deathZoneLayer");
                if (layerProp != null)
                {
                    int layer = LayerMask.NameToLayer(Layers.DeathZone);
                    if (layer >= 0 && layerProp.intValue == 0)
                    {
                        layerProp.intValue = 1 << layer;
                        so.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(config);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }

        private static void SetupPlayer()
        {
            var player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogWarning("[SetupDeathRespawn] 场景中未找到 Player");
                return;
            }

            var config = AssetDatabase.LoadAssetAtPath<DeathRespawnConfig>(ConfigPath);
            if (config == null) return;

            var controller = player.GetComponent<DeathRespawnController>();
            if (controller == null)
            {
                controller = player.AddComponent<DeathRespawnController>();
                Debug.Log("[SetupDeathRespawn] 已为 Player 添加 DeathRespawnController");
            }

            var so = new SerializedObject(controller);
            if (so.FindProperty("config").objectReferenceValue == null)
            {
                so.FindProperty("config").objectReferenceValue = config;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(controller);
            }

            // 若未配置 VFX，自动创建占位并分配（有 Debug.Log 反馈，后续可替换为真实粒子）
            var deathProp = so.FindProperty("deathVfx");
            var respawnProp = so.FindProperty("respawnVfx");
            if (deathProp.objectReferenceValue == null || respawnProp.objectReferenceValue == null)
            {
                var vfxGo = player.transform.Find("DeathRespawnVFX")?.gameObject;
                if (vfxGo == null)
                {
                    vfxGo = new GameObject("DeathRespawnVFX");
                    vfxGo.transform.SetParent(player.transform, false);
                    vfxGo.AddComponent<DeathRespawnVFXPlaceholder>();
                }
                var placeholder = vfxGo.GetComponent<DeathRespawnVFXPlaceholder>();
                if (placeholder == null)
                    placeholder = vfxGo.AddComponent<DeathRespawnVFXPlaceholder>();
                if (deathProp.objectReferenceValue == null)
                    deathProp.objectReferenceValue = placeholder;
                if (respawnProp.objectReferenceValue == null)
                    respawnProp.objectReferenceValue = placeholder;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(controller);
                Debug.Log("[SetupDeathRespawn] 已为 Player 添加 DeathRespawnVFXPlaceholder（死亡/复活时 Console 有日志，可后续替换为粒子特效）");
            }
        }
    }
}
