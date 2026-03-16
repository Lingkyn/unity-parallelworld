using UnityEngine;
using UnityEngine.SceneManagement;

namespace ParallelWorld
{
    /// <summary>
    /// 检查点存档数据，供读写使用。
    /// </summary>
    [System.Serializable]
    public struct CheckpointSaveData
    {
        public string sceneName;
        public string checkpointId;
        public float positionX;
        public float positionY;
        public float positionZ;

        public Vector3 Position => new Vector3(positionX, positionY, positionZ);

        public static CheckpointSaveData From(Vector3 position, string checkpointId, string sceneName)
        {
            return new CheckpointSaveData
            {
                sceneName = sceneName ?? "",
                checkpointId = checkpointId ?? "",
                positionX = position.x,
                positionY = position.y,
                positionZ = position.z
            };
        }
    }

    /// <summary>
    /// 最小检查点存档：保存/读取当前检查点，读档后恢复玩家位置。
    /// 保存时从 DeathRespawnController 读取；读档后场景加载完成时在 Start 中恢复检查点。
    /// </summary>
    public class SaveService : MonoBehaviour
    {
        private const string KeyScene = "checkpoint_scene";
        private const string KeyId = "checkpoint_id";
        private const string KeyX = "checkpoint_x";
        private const string KeyY = "checkpoint_y";
        private const string KeyZ = "checkpoint_z";

        private void Start()
        {
            ApplyCheckpointIfSameScene();
        }

        /// <summary>将当前场景的检查点从 Controller 写入持久化</summary>
        public void SaveCheckpoint()
        {
            var controller = ServiceLocator.Get<DeathRespawnController>();
            if (controller == null) return;

            string id = controller.GetCurrentCheckpointId();
            Vector3 pos = controller.GetCurrentCheckpointPosition();
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var data = CheckpointSaveData.From(pos, id, scene);
            WriteToPlayerPrefs(data);
        }

        /// <summary>读取已保存的检查点数据（不修改场景）</summary>
        public CheckpointSaveData? LoadCheckpointData()
        {
            if (!PlayerPrefs.HasKey(KeyScene)) return null;
            return new CheckpointSaveData
            {
                sceneName = PlayerPrefs.GetString(KeyScene),
                checkpointId = PlayerPrefs.GetString(KeyId),
                positionX = PlayerPrefs.GetFloat(KeyX),
                positionY = PlayerPrefs.GetFloat(KeyY),
                positionZ = PlayerPrefs.GetFloat(KeyZ)
            };
        }

        /// <summary>若当前场景与存档场景一致，则恢复检查点并瞬移玩家</summary>
        public void ApplyCheckpointIfSameScene()
        {
            CheckpointSaveData? data = LoadCheckpointData();
            if (data == null) return;

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (data.Value.sceneName != currentScene) return;

            var controller = ServiceLocator.Get<DeathRespawnController>();
            if (controller == null) return;

            controller.RestoreCheckpoint(data.Value.Position, data.Value.checkpointId);
        }

        private static void WriteToPlayerPrefs(CheckpointSaveData data)
        {
            PlayerPrefs.SetString(KeyScene, data.sceneName);
            PlayerPrefs.SetString(KeyId, data.checkpointId);
            PlayerPrefs.SetFloat(KeyX, data.positionX);
            PlayerPrefs.SetFloat(KeyY, data.positionY);
            PlayerPrefs.SetFloat(KeyZ, data.positionZ);
            PlayerPrefs.Save();
        }
    }
}
