using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIDropAllAdvanced : MonoBehaviour
{
    [Header("UI Group")]
    public RectTransform uiGroup;

    [Header("Canvas")]
    public RectTransform canvasRect;

    [Header("Drop Settings")]
    public float duration = 0.8f;
    public float delayMax = 0.2f;
    public float bounceHeight = 30f;

    public void DropAllUI()
    {
        float canvasHeight = canvasRect.rect.height;
        float bottomY = -canvasHeight / 2f;

        RectTransform[] allUI = uiGroup.GetComponentsInChildren<RectTransform>();

        foreach (RectTransform ui in allUI)
        {
            if (ui == uiGroup) continue;

            ui.DOKill();

            var breathing = ui.GetComponent<UIButtonBreathingEffect>();
            if (breathing != null) breathing.enabled = false;

            var shake = ui.GetComponent<UIButtonShakeEffect>();
            if (shake != null) shake.enabled = false;

            CanvasGroup cg = ui.GetComponent<CanvasGroup>();
            if (cg == null) cg = ui.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;

            float delay = Random.Range(0f, delayMax);
            float randomOffset = Random.Range(0f, 80f);
            float randomRotate = Random.Range(-25f, 25f);

            ui.DOAnchorPosY(bottomY + randomOffset, duration)
                .SetEase(Ease.InQuad)
                .SetDelay(delay)
                .OnStart(() =>
                {
                    ui.DORotate(new Vector3(0, 0, randomRotate), duration);
                })
                .OnComplete(() =>
                {
                    ui.DOAnchorPosY(bottomY + randomOffset + bounceHeight, 0.15f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            ui.DOAnchorPosY(bottomY + randomOffset, 0.1f);
                        });
                });
        }
    }
}