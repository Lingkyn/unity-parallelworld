using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIDropAllAdvanced : MonoBehaviour
{
    [Header("UI Group (只放需要掉落的UI)")]
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

            // ✅ 停掉所有Tween（关键）
            ui.DOKill();

            // ✅ 关闭所有交互动画（防止冲突）
            var breathing = ui.GetComponent<UIButtonBreathingEffect>();
            if (breathing != null) breathing.enabled = false;

            var shake = ui.GetComponent<UIButtonShakeEffect>();
            if (shake != null) shake.enabled = false;

            // ✅ 禁止鼠标再触发事件（超级关键）
            CanvasGroup cg = ui.GetComponent<CanvasGroup>();
            if (cg == null) cg = ui.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;

            // 🎲 随机参数（让动画更自然）
            float delay = Random.Range(0f, delayMax);
            float randomOffset = Random.Range(0f, 80f);
            float randomRotate = Random.Range(-25f, 25f);

            // 🎬 掉落动画
            ui.DOAnchorPosY(bottomY + randomOffset, duration)
                .SetEase(Ease.InQuad)
                .SetDelay(delay)
                .OnStart(() =>
                {
                    // 旋转
                    ui.DORotate(new Vector3(0, 0, randomRotate), duration);
                })
                .OnComplete(() =>
                {
                    // 🪀 回弹（落地感）
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