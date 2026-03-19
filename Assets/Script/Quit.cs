using UnityEngine;

public class Quit : MonoBehaviour
{
    public void OnExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else             
        Application.Quit();
#endif
    }
}