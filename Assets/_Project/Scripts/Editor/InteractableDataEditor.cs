using ParallelWorld;
using UnityEditor;
using UnityEngine;

namespace ParallelWorld.Editor
{
    [CustomEditor(typeof(InteractableData))]
    public class InteractableDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var so = serializedObject;
            so.Update();

            var sourceProp = so.FindProperty("_source");
            var source = (InteractableData.TextSource)sourceProp.enumValueIndex;
            var isTable = source == InteractableData.TextSource.Table;

            EditorGUILayout.PropertyField(sourceProp);

            var entryIdProp = so.FindProperty("_entryId");
            EditorGUILayout.PropertyField(entryIdProp);

            if (!isTable)
            {
                EditorGUILayout.PropertyField(so.FindProperty("_promptText"));
            }
            else
            {
                EditorGUILayout.PropertyField(so.FindProperty("_config"), new GUIContent("Config", "表模式时可选；不填则使用 InteractionController 的 Config"));
                EditorGUILayout.HelpBox("Prompt Text 从 Config.database 按 entryId 获取，不填 Config 则用 InteractionController 的。", MessageType.Info);
            }

            so.ApplyModifiedProperties();
        }
    }
}
