namespace ParallelWorld
{
    /// <summary>
    /// 命令模式：将请求封装为对象，便于多源触发、排队、撤销/重做、记录与回放。
    /// </summary>
    public interface ICommand
    {
        void Execute();
    }
}
