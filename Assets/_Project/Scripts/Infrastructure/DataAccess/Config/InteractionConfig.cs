using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 交互系统的可配置参数：靠近提示/按钮布局与玩家头顶对话布局。
    /// </summary>
    [CreateAssetMenu(fileName = "InteractionConfig", menuName = "ParallelWorld/Interaction Config")]
    public class InteractionConfig : ScriptableObject
    {
        [Header("检测")]
        [Tooltip("可交互物 Layer（0 表示不按层筛选）")]
        public LayerMask interactableLayer;

        [Header("靠近提示位置")]
        [Tooltip("相对可交互物锚点的世界坐标偏移；仅用于靠近文本提示与交互按钮 UI")]
        public Vector3 promptOffset = new Vector3(0f, 1f, 0f);
        [Tooltip("靠近提示与交互按钮距屏幕顶部的视口比例 0~1（如 0.833）；水平跟随锚点屏幕投影；数值更小则更靠上")]
        [Range(0f, 1f)]
        public float promptTopViewport = 0.833f;

        [Header("玩家头顶对话")]
        [Tooltip("相对当前激活 Real/Shadow 躯体的世界偏移；仅 DialogueViewController；若需与靠近提示一致可与 promptOffset 填相同值")]
        public Vector3 dialogueWorldOffset = new Vector3(0f, 1f, 0f);
        [Tooltip("玩家对话 UI 竖直视口带，含义同 promptTopViewport；仅对话；数值更小则更靠上")]
        [Range(0f, 1f)]
        public float dialogueTopViewport = 0.833f;

        [Header("默认文本")]
        [Tooltip("无 IInteractable 时的兜底提示文本")]
        public string defaultPromptText = "按 E 交互";

        [Header("数据表")]
        [Tooltip("可交互物提示文本表，多处同类型可共享 entryId")]
        public InteractableDatabase database;
        [Tooltip("按钮型可交互物表，同距离时按表序；表模式时提供 buttonText、animationClip")]
        public InteractableButtonDatabase buttonDatabase;
    }
}
