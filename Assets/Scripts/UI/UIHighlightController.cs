using UnityEngine;
using DG.Tweening;

public class UIHighlightController : MonoBehaviour
{
    public RectTransform HighlightHole;
    public CanvasGroup OverlayGroup;    // 배경 어둠 조절용 패널
    public float Duration = 0.4f;

    void Awake()
    {
        OverlayGroup.alpha = 0f;
        HighlightHole.gameObject.SetActive(false);
    }
    
    public void ApplyHighlight(TutorialStep step)
    {
        Vector2 targetScreenPos;
        Vector2 targetSize;

        if (!step.UseHighlight || step.TargetObject == null)
        {
            Hide();
            return;
        }

        // TargetObject가 UI인지 월드 오브젝트인지 판단
        RectTransform targetRect = step.TargetObject.GetComponent<RectTransform>();

        if (targetRect != null)
        {
            // UI 타겟
            targetScreenPos = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);

            float padding = step.UIPadding;
            targetSize = new Vector2(targetRect.rect.width, targetRect.rect.height) * padding;
        }
        else
        {
            // 월드 타겟
            targetScreenPos = Camera.main.WorldToScreenPoint(step.TargetObject.transform.position);

            float size = step.WorldCircleSize;
            targetSize = new Vector2(size, size);
        }

        Show(targetScreenPos, targetSize);
    }

    private void Show(Vector2 position, Vector2 size)
    {
        HighlightHole.gameObject.SetActive(true);
        OverlayGroup.DOFade(1f, Duration); 
        
        HighlightHole.DOMove(position, Duration).SetEase(Ease.OutCubic);
        HighlightHole.DOSizeDelta(size, Duration).SetEase(Ease.OutCubic);
    }

    public void Hide()
    {
        OverlayGroup.DOFade(0f, Duration);
        HighlightHole.gameObject.SetActive(false);
    }
}