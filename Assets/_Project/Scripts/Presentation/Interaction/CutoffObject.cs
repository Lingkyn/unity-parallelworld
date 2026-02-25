using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CutoffObject : MonoBehaviour
{
    [Header("Reference")]
    public Transform player;

    [Header("Fade Settings")]
    [Range(0f, 1f)]
    public float transparentAlpha = 0.35f;
    public float normalAlpha = 1f;

    private SpriteRenderer sr;
    private SpriteRenderer playerSR;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (player)
            playerSR = player.GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (!playerSR) return;

        bool overlap = sr.bounds.Intersects(playerSR.bounds);

        if (overlap)
            SetAlpha(transparentAlpha);
        else
            SetAlpha(normalAlpha);
    }

    void SetAlpha(float a)
    {
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }
}