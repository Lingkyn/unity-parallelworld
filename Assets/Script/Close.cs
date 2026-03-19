using UnityEngine;

public class Close : MonoBehaviour
{
    public Transform root;

    public void ShowWindow()
    {
        gameObject.SetActive(true);
    }

    public void HideWindow()
    {
        foreach (Transform child in root)
        {
            child.gameObject.SetActive(false);
        }
    }
}