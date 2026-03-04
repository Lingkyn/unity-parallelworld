using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 可交互物组件：支持数据表或本地文本
    /// 表模式：多个物体共享 entryId，集中配置；本地模式：每物体单独写 promptText
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableData : MonoBehaviour, IInteractable
    {
        public enum TextSource
        {
            [Tooltip("从数据表按 entryId 查找")]
            Table,
            [Tooltip("使用下方 promptText")]
            Local
        }

        [SerializeField] private TextSource _source = TextSource.Local;
        [SerializeField, Tooltip("数据表模式时使用，如 bottle、door")]
        private string _entryId;
        [SerializeField, Tooltip("表模式时可选；不填则使用 InteractionController 的 Config")]
        private InteractionConfig _config;
        [SerializeField, Tooltip("本地模式时使用，或表查找失败时的兜底")]
        private string _promptText = "按 E 交互";

        public string GetPromptText() => _promptText;

        /// <summary>
        /// 解析提示文本：优先表查找，否则本地
        /// </summary>
        public static string ResolvePromptText(GameObject go, InteractionConfig config)
        {
            if (go == null) return config?.defaultPromptText ?? "按 E 交互";

            var data = go.GetComponent<InteractableData>();
            if (data != null)
            {
                var effectiveConfig = data._config ?? config;
                if (data._source == TextSource.Table && effectiveConfig?.database != null && !string.IsNullOrEmpty(data._entryId))
                {
                    var fromTable = effectiveConfig.database.GetPromptText(data._entryId);
                    if (!string.IsNullOrEmpty(fromTable)) return fromTable;
                }
                return data._promptText ?? effectiveConfig?.defaultPromptText ?? "按 E 交互";
            }

            var interactable = go.GetComponent<IInteractable>();
            return interactable?.GetPromptText() ?? config?.defaultPromptText ?? "按 E 交互";
        }
    }
}
