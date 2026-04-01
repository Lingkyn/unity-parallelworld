using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PhotoCollect : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool collectByScreenOverlap = true;
    [SerializeField] private float screenOverlapToleranceX = 0.06f;
    [SerializeField] private float screenOverlapToleranceY = 0.08f;
    [SerializeField] private string nextSceneName;
    [SerializeField] private bool loadNextBuildIndexIfNameEmpty = true;
    [SerializeField] private AudioClip collectSfx;
    [SerializeField] private float delayBeforeSceneLoad = 0f;

    private bool collected;
    private Transform playerRootTransform;

    private void Update()
    {
        if (collected)
        {
            return;
        }

        EnsurePlayerRootTransform();
        if (playerRootTransform == null)
        {
            return;
        }

        Transform playerTransform = ResolveCurrentPlayerBodyTransform();
        if (playerTransform == null)
        {
            return;
        }

        bool screenHit = false;

        if (collectByScreenOverlap)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 playerVp = cam.WorldToViewportPoint(playerTransform.position);
                Vector3 photoVp = cam.WorldToViewportPoint(transform.position);

                if (playerVp.z > 0f && photoVp.z > 0f)
                {
                    float screenDeltaX = Mathf.Abs(playerVp.x - photoVp.x);
                    float screenDeltaY = Mathf.Abs(playerVp.y - photoVp.y);
                    screenHit = screenDeltaX <= screenOverlapToleranceX && screenDeltaY <= screenOverlapToleranceY;
                }
            }
        }

        if (screenHit)
        {
            TryCollect(playerRootTransform.gameObject);
        }
    }

    private void EnsurePlayerRootTransform()
    {
        if (playerRootTransform != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerRootTransform = player.transform;
        }
    }

    private Transform ResolveCurrentPlayerBodyTransform()
    {
        if (playerRootTransform == null)
        {
            return null;
        }

        Transform realPlayer = playerRootTransform.Find("RealPlayer");
        Transform shadowPlayer = playerRootTransform.Find("ShadowPlayer");

        if (realPlayer != null && realPlayer.gameObject.activeInHierarchy)
        {
            return realPlayer;
        }

        if (shadowPlayer != null && shadowPlayer.gameObject.activeInHierarchy)
        {
            return shadowPlayer;
        }

        return playerRootTransform;
    }

    private void TryCollect(GameObject other)
    {
        if (collected)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        collected = true;

        HidePhoto();

        if (collectSfx != null)
        {
            AudioSource.PlayClipAtPoint(collectSfx, transform.position);
        }

        StartCoroutine(CollectFlow());
    }

    private IEnumerator CollectFlow()
    {
        if (delayBeforeSceneLoad > 0f)
        {
            yield return new WaitForSeconds(delayBeforeSceneLoad);
        }

        LoadTargetScene();
    }

    private void HidePhoto()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = false;
        }

        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].enabled = false;
        }

        var colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        var colliders2D = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders2D.Length; i++)
        {
            colliders2D[i].enabled = false;
        }
    }

    private void LoadTargetScene()
    {
        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        if (!loadNextBuildIndexIfNameEmpty)
        {
            return;
        }

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
