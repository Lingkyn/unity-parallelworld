using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneJumpManager : MonoBehaviour
{
    public static SceneJumpManager Instance;

    [SerializeField] private string targetSceneName = "IntroScene02";
    [SerializeField] private float delayTime = 1f;

    private bool isJumping = false;

    private void Awake()
    {
        Instance = this;
    }

    public void JumpToIntroScene()
    {
        if (isJumping)
            return;

        isJumping = true;

        Debug.Log("【跳转管理器】检测到跳转请求，开始延迟跳转");

        StartCoroutine(JumpDelay());
    }

    private IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(delayTime);

        Debug.Log("【跳转管理器】正在跳转到场景：" + targetSceneName);

        SceneManager.LoadScene(targetSceneName);
    }
}