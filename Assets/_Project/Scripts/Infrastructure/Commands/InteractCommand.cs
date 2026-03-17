using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 对指定可交互物执行一次交互的命令（播放动画 + 锁定/解锁玩家），可由按钮点击、快捷键、剧情等统一触发。
    /// </summary>
    public class InteractCommand : ICommand
    {
        private readonly IInteractExecutor _executor;
        private readonly GameObject _target;

        public InteractCommand(IInteractExecutor executor, GameObject target)
        {
            _executor = executor;
            _target = target;
        }

        public void Execute()
        {
            _executor?.ExecuteInteraction(_target);
        }
    }
}
