using ParallelWorld;
using UnityEditor;
using UnityEngine;

namespace ParallelWorld.Editor
{
    [CustomEditor(typeof(RespawnPointDetector))]
    public class RespawnPointDetectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var so = serializedObject;
            so.Update();

            var databaseProp = so.FindProperty("checkpointDatabase");
            var tableIndexProp = so.FindProperty("tableIndex");
            var checkpointIdProp = so.FindProperty("checkpointId");
            var orderProp = so.FindProperty("order");

            EditorGUILayout.PropertyField(databaseProp);

            bool hasTable = databaseProp.objectReferenceValue != null;

            if (hasTable)
            {
                EditorGUILayout.PropertyField(tableIndexProp);
                EditorGUILayout.HelpBox("检查点 ID 与 order 从表中该条目读取，无需在此填写。", MessageType.None);
            }
            else
            {
                EditorGUILayout.PropertyField(checkpointIdProp);
                EditorGUILayout.PropertyField(orderProp);
                EditorGUILayout.HelpBox("未指定检查点表时，可在此手填 Checkpoint Id；留空则仅作复活点、不参与存档。", MessageType.None);
            }

            so.ApplyModifiedProperties();
        }
    }
}
