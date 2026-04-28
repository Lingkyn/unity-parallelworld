using UnityEngine;
using System.Collections;

public class GemTrigger : MonoBehaviour
{
    public int gemIndex;
    public GemUIController gemUI;
    public GemPopupUI popupUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(HandleGem());
        }
    }

    IEnumerator HandleGem()
    {
        // 1. 显示UI
        popupUI.ShowPopup(gemIndex);

        // 2. 等2秒
        yield return new WaitForSeconds(2f);

        // 3. 关闭UI
        popupUI.HidePopup();

        // 4. 点亮宝石
        gemUI.CollectGem(gemIndex);

        // 5. 关闭触发器
        gameObject.SetActive(false);
    }
}