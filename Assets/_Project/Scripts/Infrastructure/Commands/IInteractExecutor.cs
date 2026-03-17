using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 执行一次交互的接收者抽象：对指定 GameObject（InteractableButton）执行播放动画并锁定/解锁玩家。
    /// 供 InteractCommand 调用，便于从 UI、快捷键、剧情等多源触发同一逻辑。
    /// </summary>
    public interface IInteractExecutor
    {
        void ExecuteInteraction(GameObject target);
    }
}
