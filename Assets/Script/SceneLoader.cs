using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string nextSceneName = "Prototype 01";
    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}

