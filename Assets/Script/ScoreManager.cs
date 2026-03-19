using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int score = 5;
    public Text scoreText; 

    private void Start()
    {
        UpdateScoreText();
    }

    public void ReduceScore(int value)
    {
        score -= value;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }
}

