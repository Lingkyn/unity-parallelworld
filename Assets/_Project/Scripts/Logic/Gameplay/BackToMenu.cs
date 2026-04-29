using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenu : MonoBehaviour
{
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("UI Anim Test");
    }
}