using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 死亡复活系统配置
    /// </summary>
    [CreateAssetMenu(fileName = "DeathRespawnConfig", menuName = "ParallelWorld/Death Respawn Config")]
    public class DeathRespawnConfig : ScriptableObject
    {
        [Header("检测")]
        [Tooltip("死亡区域所在 Layer（Editor 设置用）")]
        public LayerMask deathZoneLayer;
        [Tooltip("复活点 Tag，默认 Respawn（Unity 自带）")]
        public string respawnTag = "Respawn";
        [Tooltip("玩家 Tag，默认 Player（Unity 自带）")]
        public string playerTag = "Player";

        [Header("复活")]
        [Tooltip("无复活点时的默认出生坐标")]
        public Vector3 defaultSpawnPosition = Vector3.zero;
        [Tooltip("死亡特效播放后等待时长（秒），再瞬移复活。0 则立即复活")]
        public float deathToRespawnDelay = 0.8f;
        [Tooltip("复活特效播放后等待时长（秒），再恢复角色控制。0 则立即恢复")]
        public float respawnToControlDelay = 0.3f;
        [Tooltip("复活后无敌时长（秒）")]
        public float respawnInvincibleDuration = 0.5f;
    }
}
