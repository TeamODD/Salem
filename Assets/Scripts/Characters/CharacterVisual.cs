using DG.Tweening;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class CharacterVisual : MonoBehaviour
{
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private Ease scaleEase = Ease.Linear;

    [Header("Sorting Settings")]
    [SerializeField] private int defaultSortingOrder = 0;
    [SerializeField] private int focusedSortingOrder = 100;

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScaleFactor;
    private float hoverFactor;
    private bool isFocused = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = defaultSortingOrder;
        baseScaleFactor = Vector3.one;
    }

    public void Initialize(CharacterData data)
    {
        if (data == null) return;

        if (data.defaultSprite != null)
        {
            spriteRenderer.sprite = data.defaultSprite;
        }

        baseScaleFactor = new Vector3(data.baseScale, data.baseScale, 1f);

        transform.localScale = baseScaleFactor;

        hoverFactor = data.hoverScaleFactor;
    }

    public void SetFocus(bool focus)
    {
        if (isFocused == focus) return;

        isFocused = focus;
        transform.DOKill();

        if (focus)
        {
            spriteRenderer.sortingOrder = focusedSortingOrder;
            transform.DOScale(baseScaleFactor * hoverFactor, animationDuration)
                     .SetEase(scaleEase);

            if (GlobalFadeManager.Instance != null)
                GlobalFadeManager.Instance.SetFocus(true);
        }
        else
        {
            spriteRenderer.sortingOrder = defaultSortingOrder;
            transform.DOScale(baseScaleFactor, animationDuration)
                     .SetEase(Ease.OutQuad);

            if (GlobalFadeManager.Instance != null)
                GlobalFadeManager.Instance.SetFocus(false);
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}

