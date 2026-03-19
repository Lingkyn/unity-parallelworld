using UnityEngine;

public class WindowController : MonoBehaviour
{
    public GameObject popupWindow;

    public void ShowWindow()
    {
        popupWindow.SetActive(true);
    }

    public void HideWindow()
    {
        popupWindow.SetActive(false);
    }
}