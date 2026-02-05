using UnityEngine;
using TMPro;
using System;
using DG.Tweening;

[Serializable]
public class TutorialStep
{
    [TextArea(3, 10)]
    public string Message;
    public Vector2 DialoguePosition;
    public RectTransform TargetUI;
    public Transform TargetWorld;

    public bool UseHighlight = true;
    public float WorldCircleSize = 300f;
    public float Padding = 1.2f;
}

public class TutorialManager : MonoBehaviour
{
    public TutorialStep[] Steps;

    [Header("References")]
    public RectTransform DialogueBox;
    public UIHighlightController UIHighlight;
    public TextMeshProUGUI DescText;

    [Header("Placement Settings")]
    public float HorizontalOffset = 200f; // 타겟 중심으로부터의 거리
    public float VerticalOffset = 0f;     // 높이 미세 조정
    public float MoveDuration = 0.4f;

    private int _currentIndex = 0;

    void Start()
    {
        if (Steps != null && Steps.Length > 0) ShowStep(_currentIndex);
    }

    public void OnNextStep()
    {
        _currentIndex++;
        if (_currentIndex < Steps.Length) ShowStep(_currentIndex);
        else EndTutorial();
    }

    private void ShowStep(int index)
    {
        TutorialStep step = Steps[index];
        DescText.text = step.Message;

        Vector2 targetScreenPos = Vector2.zero;
        bool hasTarget = false;

        if (step.TargetUI != null)
        {
            targetScreenPos = RectTransformUtility.WorldToScreenPoint(null, step.TargetUI.position);
            hasTarget = true;
        }
        else if (step.TargetWorld != null)
        {
            targetScreenPos = Camera.main.WorldToScreenPoint(step.TargetWorld.position);
            hasTarget = true;
        }

        // 대화창 위치 계산
        Vector2 finalAnchoredPos;

        if (hasTarget)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                DialogueBox.parent as RectTransform, 
                targetScreenPos, 
                null, 
                out Vector2 localPos);

            float direction = (targetScreenPos.x < Screen.width / 2f) ? 1f : -1f;
            
            finalAnchoredPos = new Vector2(localPos.x + (HorizontalOffset * direction), localPos.y + VerticalOffset);
        }
        else
        {
            finalAnchoredPos = step.DialoguePosition;
        }

        // 부드럽게 이동
        DialogueBox.DOAnchorPos(finalAnchoredPos, MoveDuration).SetEase(Ease.OutCubic);

        // 하이라이트 실행
        if (step.UseHighlight && hasTarget) UIHighlight.ApplyHighlight(step);
        else UIHighlight.Hide();
    }

    private void EndTutorial()
    {
        UIHighlight.Hide();
        gameObject.SetActive(false);
    }
}