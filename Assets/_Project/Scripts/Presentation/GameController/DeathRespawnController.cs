using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 死亡复活控制器：挂 Player 根，接收 DeathZoneDetector/RespawnPointDetector 回调，
    /// 编排死亡→瞬移→复活流程，禁用/恢复 Movement、Interaction、PlayerToggle
    /// </summary>
    public class DeathRespawnController : MonoBehaviour
    {
        [Header("需在 Inspector 配置")]
        [SerializeField] private DeathRespawnConfig config;
        [SerializeField, Tooltip("可选：死亡特效，有则拖入")]
        private MonoBehaviour deathVfx;
        [SerializeField, Tooltip("可选：复活特效，有则拖入")]
        private MonoBehaviour respawnVfx;

        private MovementController movementController;
        private InteractionController interactionController;
        private PlayerToggleController playerToggleController;

        private DeathRespawnSystem _system;
        private IDeathVFX _deathVfx;
        private IRespawnVFX _respawnVfx;
        private float _invincibleTimer;
        private bool _isRespawning;

        private void Awake()
        {
            movementController = GetComponent<MovementController>();
            interactionController = GetComponent<InteractionController>();
            playerToggleController = GetComponent<PlayerToggleController>();

            if (config == null)
                Debug.LogWarning("[DeathRespawnController] DeathRespawnConfig 未分配");

            _system = new DeathRespawnSystem();
            Vector3 defaultPos = config != null ? config.defaultSpawnPosition : Vector3.zero;
            var firstRespawn = GameObject.FindGameObjectWithTag(Tags.Respawn);
            if (firstRespawn != null)
                defaultPos = firstRespawn.transform.position;
            _system.SetDefaultSpawnPosition(defaultPos);
            _deathVfx = deathVfx as IDeathVFX;
            _respawnVfx = respawnVfx as IRespawnVFX;
        }

        private void Update()
        {
            if (_invincibleTimer > 0f)
                _invincibleTimer -= Time.deltaTime;
        }

        /// <summary>
        /// 由 DeathZoneDetector 调用：玩家进入死亡区域时触发
        /// </summary>
        public void HandleDeathRequested()
        {
            if (_invincibleTimer > 0f)
                return;
            if (config == null)
                return;
            if (_isRespawning)
                return;

            StartCoroutine(ExecuteDeathRespawnCoroutine());
        }

        /// <summary>
        /// 由 RespawnPointDetector 调用：玩家经过复活点时激活
        /// </summary>
        public void HandleRespawnPointActivated(Vector3 position, string checkpointId = null)
        {
            _system?.NotifyActivateRespawnPoint(position, checkpointId);
        }

        /// <summary>
        /// 供存档系统：设置 Config、恢复默认出生点（如场景加载后）
        /// </summary>
        public void SetConfig(DeathRespawnConfig newConfig)
        {
            config = newConfig;
            if (_system != null && config != null)
                _system.SetDefaultSpawnPosition(config.defaultSpawnPosition);
        }

        private System.Collections.IEnumerator ExecuteDeathRespawnCoroutine()
        {
            if (movementController == null || config == null)
                yield break;

            _isRespawning = true;
            try
            {
                Vector3 deathPos = transform.position;
                Vector3 forward = transform.forward;

                // 1. 禁用三 Controller
                if (movementController != null) movementController.enabled = false;
                if (interactionController != null) interactionController.enabled = false;
                if (playerToggleController != null) playerToggleController.enabled = false;

                // 2. 死亡 VFX
                if (_deathVfx != null)
                    _deathVfx.OnDeath(deathPos, forward);

                // 3. 等待死亡特效播放
                float delay = Mathf.Max(0f, config.deathToRespawnDelay);
                if (delay > 0f)
                    yield return new WaitForSeconds(delay);

                // 4. 获取复活位置并瞬移
                Vector3 respawnPos = _system.GetRespawnPosition();
                movementController.TeleportTo(respawnPos);

                // 5. 复活 VFX
                if (_respawnVfx != null)
                    _respawnVfx.OnRespawn(respawnPos, forward);

                // 6. 等待后再恢复控制
                float controlDelay = Mathf.Max(0f, config.respawnToControlDelay);
                if (controlDelay > 0f)
                    yield return new WaitForSeconds(controlDelay);

                // 7. 恢复三 Controller
                if (movementController != null) movementController.enabled = true;
                if (interactionController != null) interactionController.enabled = true;
                if (playerToggleController != null) playerToggleController.enabled = true;

                // 8. 启动无敌计时
                _invincibleTimer = config.respawnInvincibleDuration;
            }
            finally
            {
                _isRespawning = false;
            }
        }
    }
}
