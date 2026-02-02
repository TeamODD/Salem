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

    private SpriteRenderer _spriteRenderer;
    private Vector3 _baseScaleFactor;
    private float _hoverFactor;
    private bool _isFocused = false;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sortingOrder = defaultSortingOrder;
        _baseScaleFactor = Vector3.one;
    }

    public void Initialize(CharacterData data)
    {
        if (data == null) return;

        if (data.defaultSprite != null)
        {
            _spriteRenderer.sprite = data.defaultSprite;
        }

        _baseScaleFactor = new Vector3(data.baseScale, data.baseScale, 1f);

        transform.localScale = _baseScaleFactor;

        _hoverFactor = data.hoverScaleFactor;
    }

    public void SetFocus(bool focus)
    {
        if (_isFocused == focus) return;

        _isFocused = focus;
        transform.DOKill();

        if (focus)
        {
            _spriteRenderer.sortingOrder = focusedSortingOrder;
            transform.DOScale(_baseScaleFactor * _hoverFactor, animationDuration)
                     .SetEase(scaleEase);

            if (GlobalFadeManager.Instance != null)
                GlobalFadeManager.Instance.SetFocus(true);
        }
        else
        {
            _spriteRenderer.sortingOrder = defaultSortingOrder;
            transform.DOScale(_baseScaleFactor, animationDuration)
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

