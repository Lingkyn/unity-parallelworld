namespace ParallelWorld
{
    /// <summary>
    /// 玩家形态切换的接收者抽象，供 Command 与 UI/脚本等调用，不依赖具体 MonoBehaviour。
    /// </summary>
    public interface IPlayerFormSwitcher
    {
        void Toggle();
        void SetActiveForm(PlayerForm form);
        PlayerForm CurrentForm { get; }
    }
}
