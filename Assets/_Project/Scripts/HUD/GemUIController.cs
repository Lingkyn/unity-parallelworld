using UnityEngine;
using UnityEngine.UI;

public class GemUIController : MonoBehaviour
{
    public Image[] gemImages;

    private bool[] collected;

    private void Start()
    {
        collected = new bool[gemImages.Length];

        for (int i = 0; i < gemImages.Length; i++)
        {
            gemImages[i].color = Color.gray;
        }
    }

    public void CollectGem(int index)
    {
        if (index < 0 || index >= gemImages.Length)
            return;

        if (collected[index])
            return;

        collected[index] = true;
        gemImages[index].color = Color.white;
    }
}