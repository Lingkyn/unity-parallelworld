namespace ParallelWorld
{
    /// <summary>
    /// 将玩家躯体设置为指定形态的命令，可由 UI、剧情、读档等触发。
    /// </summary>
    public class SetPlayerFormCommand : ICommand
    {
        private readonly IPlayerFormSwitcher _switcher;
        private readonly PlayerForm _form;

        public SetPlayerFormCommand(IPlayerFormSwitcher switcher, PlayerForm form)
        {
            _switcher = switcher;
            _form = form;
        }

        public void Execute()
        {
            _switcher?.SetActiveForm(_form);
        }
    }
}
