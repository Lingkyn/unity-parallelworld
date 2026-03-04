using ParallelWorld;
using UnityEditor;
using UnityEngine;

namespace ParallelWorld.Editor
{
    [CustomEditor(typeof(InteractableButtonData))]
    public class InteractableButtonDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var so = serializedObject;
            so.Update();

            var sourceProp = so.FindProperty("_source");
            var source = (InteractableButtonData.ButtonSource)sourceProp.enumValueIndex;
            var isTable = source == InteractableButtonData.ButtonSource.Table;

            EditorGUILayout.PropertyField(sourceProp);

            var entryIdProp = so.FindProperty("_entryId");
            EditorGUILayout.PropertyField(entryIdProp);

            if (!isTable)
            {
                EditorGUILayout.PropertyField(so.FindProperty("_buttonText"));
                EditorGUILayout.PropertyField(so.FindProperty("_animationClip"));
                EditorGUILayout.PropertyField(so.FindProperty("_defaultStateName"));
            }
            else
            {
                EditorGUILayout.PropertyField(so.FindProperty("_config"), new GUIContent("Config", "表模式必填，引用 InteractionConfig（含 buttonDatabase）"));
                EditorGUILayout.HelpBox("Button Text、Animation Clip、Default State Name 均从 InteractableButtonDatabase 按 entryId 获取", MessageType.Info);
            }

            EditorGUILayout.PropertyField(so.FindProperty("_animator"));

            so.ApplyModifiedProperties();
        }
    }
}
