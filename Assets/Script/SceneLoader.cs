using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneLoader : MonoBehaviour
{
    public string nextSceneName = "Prototype 01";
    public float delay = 2f;

    public void LoadNextScene()
    {
        Invoke("LoadScene", delay);
    }

    void LoadScene()
    {
        DOTween.KillAll(); // ✅ 关键：先杀掉所有动画
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}