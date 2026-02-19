using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ParallelWorld.Editor
{
    /// <summary>
    /// 为场景中的 GameplaySetup 添加光圈控制组件，并关联 Circle
    /// </summary>
    public static class SetupAperture
    {
        [MenuItem("Tools/Setup Aperture")]
        public static void Execute()
        {
            var circle = GameObject.Find("Circle");
            if (circle == null)
            {
                Debug.LogWarning("[SetupAperture] 场景中未找到名为 'Circle' 的对象");
                return;
            }

            var setup = GameObject.Find("GameplaySetup");
            if (setup == null)
            {
                Debug.LogWarning("[SetupAperture] 场景中未找到 GameplaySetup");
                return;
            }

            bool dirty = false;

            LightInputAdapter adapter = setup.GetComponent<LightInputAdapter>();
            if (adapter == null)
            {
                adapter = setup.AddComponent<LightInputAdapter>();
                var inputAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
                    "Assets/_Project/Scripts/Presentation/Input/PlayerInputAction.inputactions");
                if (inputAsset != null)
                {
                    SerializedObject so = new SerializedObject(adapter);
                    so.FindProperty("inputActions").objectReferenceValue = inputAsset;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                dirty = true;
            }

            if (setup.GetComponent<ApertureController>() == null)
            {
                var ctrl = setup.AddComponent<ApertureController>();
                var config = AssetDatabase.LoadAssetAtPath<ApertureConfig>("Assets/_Project/Data/ApertureConfig.asset");
                if (config == null)
                {
                    CreateDefaultConfigs.Execute();
                    config = AssetDatabase.LoadAssetAtPath<ApertureConfig>("Assets/_Project/Data/ApertureConfig.asset");
                }
                SerializedObject so = new SerializedObject(ctrl);
                so.FindProperty("apertureTarget").objectReferenceValue = circle.transform;
                if (config != null)
                    so.FindProperty("config").objectReferenceValue = config;
                so.FindProperty("lightInputAdapter").objectReferenceValue = adapter;
                so.ApplyModifiedPropertiesWithoutUndo();
                dirty = true;
            }

            if (dirty)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[SetupAperture] 已在 GameplaySetup 上添加 ApertureController 和 LightInputAdapter，并关联 Circle");
            }
            else
            {
                Debug.Log("[SetupAperture] GameplaySetup 已有光圈组件");
            }
        }
    }
}
