using UnityEditor;
using UnityEngine;
using Unity.Cinemachine;

namespace ParallelWorld.Editor
{
    /// <summary>
    /// 一键设置 Cinemachine：为主相机添加 CinemachineBrain，创建 Virtual Camera 跟随 Player
    /// 在场景中有 GameplaySetup 或 Player 时运行：Tools -> Setup Cinemachine
    /// </summary>
    public static class SetupCinemachine
    {
        [MenuItem("Tools/Setup Cinemachine")]
        public static void Execute()
        {
            var brain = Object.FindFirstObjectByType<CinemachineBrain>();
            if (brain == null)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    cam.gameObject.AddComponent<CinemachineBrain>();
                    Debug.Log("[SetupCinemachine] 已为主相机添加 CinemachineBrain");
                }
            }

            var vcam = Object.FindFirstObjectByType<CinemachineCamera>();
            if (vcam == null)
            {
                var player = GameObject.Find("Player");
                if (player != null)
                {
                    var go = new GameObject("CinemachineCamera");
                    vcam = go.AddComponent<CinemachineCamera>();
                    vcam.Follow = player.transform;
                    vcam.LookAt = player.transform;
                    Debug.Log("[SetupCinemachine] 已创建 Cinemachine Virtual Camera，Follow/LookAt = Player");
                }
                else
                {
                    Debug.LogWarning("[SetupCinemachine] 场景中未找到 Player，请手动设置 Virtual Camera 的 Follow/LookAt");
                }
            }
        }
    }
}
