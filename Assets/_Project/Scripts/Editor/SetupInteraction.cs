using ParallelWorld;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace ParallelWorld.Editor
{
    /// <summary>
    /// 设置交互系统：创建 Prompt UIDocument、TriggerDetector、InteractionController
    /// 在场景中有 GameplaySetup 和 Player 时运行：Tools -> Setup Interaction
    /// </summary>
    public static class SetupInteraction
    {
        private const string PromptUxmlPath = "Assets/_Project/UI/Documents/Prompt.uxml";
        private const string PanelSettingsPath = "Assets/UI Toolkit/PanelSettings.asset";

        [MenuItem("Tools/Setup Interaction")]
        public static void Execute()
        {
            var setup = GameObject.Find("GameplaySetup");
            if (setup == null)
            {
                Debug.LogWarning("[SetupInteraction] 场景中未找到 GameplaySetup");
                return;
            }

            var player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogWarning("[SetupInteraction] 场景中未找到 Player");
                return;
            }

            bool dirty = false;

            // 1. 创建 InteractionConfig 和 InteractableDatabase
            var config = AssetDatabase.LoadAssetAtPath<InteractionConfig>("Assets/_Project/Data/InteractionConfig.asset");
            var database = AssetDatabase.LoadAssetAtPath<InteractableDatabase>("Assets/_Project/Data/InteractableDatabase.asset");
            if (config == null || database == null)
            {
                CreateDefaultConfigs.Execute();
                if (config == null) config = AssetDatabase.LoadAssetAtPath<InteractionConfig>("Assets/_Project/Data/InteractionConfig.asset");
                if (database == null) database = AssetDatabase.LoadAssetAtPath<InteractableDatabase>("Assets/_Project/Data/InteractableDatabase.asset");
            }
            if (config != null && database != null && config.database == null)
            {
                var configSo = new SerializedObject(config);
                configSo.FindProperty("database").objectReferenceValue = database;
                configSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(config);
                dirty = true;
            }

            // 2. 创建或查找 PromptUI（UIDocument + PromptViewController）
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PromptUxmlPath);
            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);

            UIDocument uiDoc = Object.FindAnyObjectByType<UIDocument>(FindObjectsInactive.Include);
            GameObject promptUI = null;
            if (uiDoc != null && uiDoc.visualTreeAsset != null &&
                uiDoc.visualTreeAsset.name.Contains("Prompt"))
            {
                promptUI = uiDoc.gameObject;
            }
            else if (uiDoc != null && uxml != null && uiDoc.visualTreeAsset == uxml)
            {
                promptUI = uiDoc.gameObject;
            }

            if (promptUI == null)
            {
                promptUI = new GameObject("PromptUI");
                promptUI.transform.SetParent(setup.transform, false);
                uiDoc = promptUI.AddComponent<UIDocument>();
                if (uxml != null) uiDoc.visualTreeAsset = uxml;
                if (panelSettings != null) uiDoc.panelSettings = panelSettings;
                dirty = true;
            }
            else
            {
                uiDoc = promptUI.GetComponent<UIDocument>();
                if (uiDoc != null && (uiDoc.visualTreeAsset == null || !uiDoc.visualTreeAsset.name.Contains("Prompt")))
                {
                    if (uxml != null) uiDoc.visualTreeAsset = uxml;
                    if (panelSettings != null) uiDoc.panelSettings = panelSettings;
                    dirty = true;
                }
            }

            var promptVC = promptUI.GetComponent<PromptViewController>();
            if (promptVC == null)
            {
                promptVC = promptUI.AddComponent<PromptViewController>();
                dirty = true;
            }

            // 3. 创建 Player 的 Trigger 检测体子物体
            var detector = player.transform.Find("InteractionDetector");
            if (detector == null)
            {
                var go = new GameObject("InteractionDetector");
                go.transform.SetParent(player.transform, false);
                go.transform.localPosition = Vector3.zero;
                detector = go.transform;

                var col = go.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 2f;

                var triggerDet = go.AddComponent<TriggerDetector>();
                if (config != null)
                    triggerDet.SetInteractableTag(config.interactableTag);
                dirty = true;
            }
            else if (detector.GetComponent<TriggerDetector>() == null)
            {
                var col = detector.GetComponent<Collider>();
                if (col == null)
                {
                    col = detector.gameObject.AddComponent<SphereCollider>();
                    col.isTrigger = true;
                    ((SphereCollider)col).radius = 2f;
                }
                else
                    col.isTrigger = true;

                var triggerDet = detector.gameObject.AddComponent<TriggerDetector>();
                if (config != null)
                    triggerDet.SetInteractableTag(config.interactableTag);
                dirty = true;
            }

            var triggerDetector = detector.GetComponent<TriggerDetector>();

            // 4. 添加 InteractionController 到 GameplaySetup
            var interactionCtrl = setup.GetComponent<InteractionController>();
            if (interactionCtrl == null)
            {
                interactionCtrl = setup.AddComponent<InteractionController>();
                dirty = true;
            }

            var so = new SerializedObject(interactionCtrl);
            so.FindProperty("_triggerDetector").objectReferenceValue = triggerDetector;
            so.FindProperty("_promptViewController").objectReferenceValue = promptVC;
            so.FindProperty("_config").objectReferenceValue = config;
            so.FindProperty("_camera").objectReferenceValue = Camera.main;
            so.ApplyModifiedPropertiesWithoutUndo();

            if (dirty)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[SetupInteraction] 已配置交互系统：PromptUI、InteractionDetector、InteractionController");
            }
            else
            {
                Debug.Log("[SetupInteraction] 交互系统已就绪，已更新引用");
            }

            Debug.Log("[SetupInteraction] 提示：1) Project Settings -> Tags 添加 'Interactable'；2) 可交互物体添加 Tag、Collider、InteractableData；3) 多处共享提示时用 Table 模式 + InteractableDatabase 填写 entryId");
        }
    }
}
