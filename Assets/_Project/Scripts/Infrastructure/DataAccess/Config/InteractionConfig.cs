using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 交互系统的可配置参数（Proximity Prompt）
    /// </summary>
    [CreateAssetMenu(fileName = "InteractionConfig", menuName = "ParallelWorld/Interaction Config")]
    public class InteractionConfig : ScriptableObject
    {
        [Header("检测")]
        [Tooltip("可交互物 Layer（0 表示不按层筛选）")]
        public LayerMask interactableLayer;

        [Header("提示位置")]
        [Tooltip("提示框相对对象头顶的世界坐标偏移")]
        public Vector3 promptOffset = new Vector3(0f, 1f, 0f);
        [Tooltip("InteractableButton/InteractableText UI 距屏幕顶部的固定像素值（Y 轴固定，X 随对象变化）")]
        public float fixedPromptTopPx = 900f;

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
