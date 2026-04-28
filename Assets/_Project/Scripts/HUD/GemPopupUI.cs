using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GemPopupUI : MonoBehaviour
{
    public GameObject panel;
    public Image gemIcon;
    public TextMeshProUGUI gemText;

    public Sprite rubySprite;
    public Sprite yellowSprite;
    public Sprite violetSprite;

    public void ShowPopup(int index)
    {
        panel.SetActive(true);

        switch (index)
        {
            case 0:
                gemIcon.sprite = rubySprite;
                gemText.text = "Anxiety";
                break;
            case 1:
                gemIcon.sprite = yellowSprite;
                gemText.text = "Fear";
                break;
            case 2:
                gemIcon.sprite = violetSprite;
                gemText.text = "Sadness";
                break;
        }
    }

    public void HidePopup()
    {
        panel.SetActive(false);
    }
}