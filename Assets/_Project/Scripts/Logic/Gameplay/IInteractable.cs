namespace ParallelWorld
{
    /// <summary>
    /// 可交互物接口，提供 Proximity Prompt 的提示文本
    /// 挂于可交互物上的组件需实现此接口
    /// </summary>
    public interface IInteractable
    {
        string GetPromptText();
    }
}
