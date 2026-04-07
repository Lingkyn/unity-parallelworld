using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonShakeEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;

    [Header("Shake Settings")]
    public float duration = 0.4f;
    public float strength = 8f;
    public int vibrato = 20;

    private Vector3 basePosition;

    void Start()
    {
        image = GetComponent<Image>();
        basePosition = transform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();

        // 抖动效果
        transform.DOShakePosition(duration, strength, vibrato, 90, false, true)
            .SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();

        // 回到原位
        transform.DOLocalMove(basePosition, 0.2f).SetEase(Ease.OutQuad);
    }
}