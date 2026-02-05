using UnityEngine;
using DG.Tweening;

public class UIHighlightController : MonoBehaviour
{
    public RectTransform HighlightHole;
    public CanvasGroup OverlayGroup;    // 배경 어둠 조절용 패널
    public float Duration = 0.4f;

    public void ApplyHighlight(TutorialStep step)
    {
        Vector2 targetScreenPos;
        Vector2 targetSize;

        if (!step.UseHighlight)
        {
            Hide();
            return;
        }

        if (step.TargetUI != null)
        {
            targetScreenPos = RectTransformUtility.WorldToScreenPoint(null, step.TargetUI.position);
            targetSize = new Vector2(step.TargetUI.rect.width, step.TargetUI.rect.height) * step.Padding;
        }

        else if (step.TargetWorld != null)
        {
            targetScreenPos = Camera.main.WorldToScreenPoint(step.TargetWorld.position);
            targetSize = new Vector2(step.WorldCircleSize, step.WorldCircleSize);
        }

        else
        {
            Hide();
            return;
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