namespace ParallelWorld
{
    /// <summary>
    /// 切换玩家躯体形态（Real/Shadow）的命令，可由按键、UI、剧情等统一触发。
    /// </summary>
    public class TogglePlayerFormCommand : ICommand
    {
        private readonly IPlayerFormSwitcher _switcher;

        public TogglePlayerFormCommand(IPlayerFormSwitcher switcher)
        {
            _switcher = switcher;
        }

        public void Execute()
        {
            _switcher?.Toggle();
        }
    }
}
