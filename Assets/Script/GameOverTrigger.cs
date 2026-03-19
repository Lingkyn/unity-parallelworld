using UnityEngine;
using UnityEngine.UI;

public class GameOverTrigger : MonoBehaviour
{
    public GameObject gameOverUI;
    public ScoreManager scoreManager;
    public Text finalScoreText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Main Car")
        {

            gameOverUI.SetActive(true);

            finalScoreText.text = scoreManager.score.ToString();

            Time.timeScale = 0f;
        }
    }
}
