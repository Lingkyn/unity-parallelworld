using UnityEngine;

public class PenaltyUIManager : MonoBehaviour
{
    public ScoreManager scoreManager;

    [System.Serializable]
    public class PenaltyPanel
    {
        public GameObject panel; 
        public int penalty = 1;

        [HideInInspector] public bool wasActive = false; 
    }

    public PenaltyPanel[] panels;

    void Update()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            bool nowOpen = panels[i].panel.activeSelf;

            if (panels[i].wasActive == false && nowOpen == true)
            {
                scoreManager.ReduceScore(panels[i].penalty);
            }

            panels[i].wasActive = nowOpen;
        }
    }

}
