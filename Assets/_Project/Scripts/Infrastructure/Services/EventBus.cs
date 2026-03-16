using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 全局事件总线：集中定义游戏内事件，供发布/订阅，实现观察者模式解耦。
    /// 死亡/复活/检查点/读档/形态切换等事件在此发布，各系统订阅即可响应。
    /// </summary>
    public static class EventBus
    {
        // ----- 死亡/复活 -----

        /// <summary>玩家进入死亡区域时由 DeathZoneDetector 发布，DeathRespawnController 订阅并执行死亡流程。</summary>
        public static event System.Action PlayerDeathRequested;

        /// <summary>死亡流程开始（真正进入死亡协程）时由 DeathRespawnController 发布，供 UI/音效/统计订阅。</summary>
        public static event System.Action<Vector3> PlayerDied;

        /// <summary>复活完成、控制恢复后由 DeathRespawnController 发布，供 UI/音效/统计订阅。</summary>
        public static event System.Action<Vector3> PlayerRespawned;

        // ----- 检查点 -----

        /// <summary>玩家经过复活点时由 RespawnPointDetector 发布，参数：位置、检查点 ID、order。DeathRespawnController 订阅并激活检查点。</summary>
        public static event System.Action<Vector3, string, int> RespawnPointActivated;

        /// <summary>检查点首次被激活时由 DeathRespawnController 发布（含位置与 ID），供 UI/成就等订阅。</summary>
        public static event System.Action<Vector3, string> CheckpointActivated;

        /// <summary>读档并应用检查点后由 SaveService 发布，供 UI/成就等订阅。</summary>
        public static event System.Action<CheckpointSaveData> CheckpointRestored;

        // ----- 玩家形态 -----

        /// <summary>Real/Shadow 形态切换后由 PlayerToggleController 发布，参数：前一形态、当前形态。</summary>
        public static event System.Action<PlayerForm, PlayerForm> PlayerFormChanged;

        // ----- 发布方法 -----

        public static void PublishPlayerDeathRequested() => PlayerDeathRequested?.Invoke();
        public static void PublishPlayerDied(Vector3 deathPos) => PlayerDied?.Invoke(deathPos);
        public static void PublishPlayerRespawned(Vector3 respawnPos) => PlayerRespawned?.Invoke(respawnPos);
        public static void PublishRespawnPointActivated(Vector3 position, string checkpointId, int order) => RespawnPointActivated?.Invoke(position, checkpointId, order);
        public static void PublishCheckpointActivated(Vector3 position, string checkpointId) => CheckpointActivated?.Invoke(position, checkpointId);
        public static void PublishCheckpointRestored(CheckpointSaveData data) => CheckpointRestored?.Invoke(data);
        public static void PublishPlayerFormChanged(PlayerForm previous, PlayerForm current) => PlayerFormChanged?.Invoke(previous, current);
    }
}
