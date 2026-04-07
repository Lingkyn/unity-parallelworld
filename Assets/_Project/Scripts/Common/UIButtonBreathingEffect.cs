using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonBreathingEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;

    [Header("Breathing Settings")]
    [Range(1.0f, 1.2f)]
    public float scaleMultiplier = 1.03f; // 呼吸幅度（建议 1.02 ~ 1.05）

    public float duration = 1.2f; // 呼吸速度（越大越慢）

    private Vector3 baseScale;
    private Color originalColor;

    void Start()
    {
        image = GetComponent<Image>();

        // ? 记录当前真实大小（不管你放大多少都正确）
        baseScale = transform.localScale;

        // 记录颜色
        originalColor = image.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();

        // ? 基于当前大小做呼吸（不会跳变）
        transform.DOScale(baseScale * scaleMultiplier, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // ?? 轻微颜色变化（更柔和）
        image.DOColor(new Color(1f, 0.85f, 0.85f), 0.3f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();

        // 恢复原始大小
        transform.DOScale(baseScale, 0.3f).SetEase(Ease.OutQuad);

        // 恢复颜色
        image.DOColor(originalColor, 0.3f);
    }
}