using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreSummaryUI : MonoBehaviour
{
    [Header("UI 텍스트 (단일)")]
    [SerializeField] private TextMeshProUGUI roundsText;
    [SerializeField] private TextMeshProUGUI sacrificedText;
    [SerializeField] private TextMeshProUGUI correctMemoText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI gradeText;

    [Header("UI 텍스트 (분리: 라벨/값)")]
    [SerializeField] private TextMeshProUGUI roundsLabelText;
    [SerializeField] private TextMeshProUGUI roundsValueText;
    [SerializeField] private TextMeshProUGUI sacrificedLabelText;
    [SerializeField] private TextMeshProUGUI sacrificedValueText;
    [SerializeField] private TextMeshProUGUI correctMemoLabelText;
    [SerializeField] private TextMeshProUGUI correctMemoValueText;
    [SerializeField] private TextMeshProUGUI totalScoreLabelText;
    [SerializeField] private TextMeshProUGUI totalScoreValueText;
    [SerializeField] private TextMeshProUGUI gradeLabelText;
    [SerializeField] private TextMeshProUGUI gradeValueText;

    [Header("테스트 입력값 (게임 로직 미연동)")]
    [SerializeField] private int testRoundsCompleted;
    [SerializeField] private int testSacrificedExcludingWitch;
    [SerializeField] private int testCorrectMemoCount;

    [Header("표시 연출")]
    [SerializeField] private bool useSequentialReveal = true;
    [SerializeField] private float revealInterval = 0.35f;

    [Header("타이틀 돌아가기 버튼")]
    [SerializeField] private Button returnToTitleButton;

    public void ConfigureBindings(
        TextMeshProUGUI roundsLabel,
        TextMeshProUGUI roundsValue,
        TextMeshProUGUI sacrificedLabel,
        TextMeshProUGUI sacrificedValue,
        TextMeshProUGUI correctMemoLabel,
        TextMeshProUGUI correctMemoValue,
        TextMeshProUGUI totalScoreLabel,
        TextMeshProUGUI totalScoreValue,
        TextMeshProUGUI gradeLabel,
        TextMeshProUGUI gradeValue,
        Button titleButton)
    {
        roundsLabelText = roundsLabel;
        roundsValueText = roundsValue;
        sacrificedLabelText = sacrificedLabel;
        sacrificedValueText = sacrificedValue;
        correctMemoLabelText = correctMemoLabel;
        correctMemoValueText = correctMemoValue;
        totalScoreLabelText = totalScoreLabel;
        totalScoreValueText = totalScoreValue;
        gradeLabelText = gradeLabel;
        gradeValueText = gradeValue;
        returnToTitleButton = titleButton;
    }

    private void Start()
    {
        SetReturnToTitleVisible(false);
        RefreshWithTestValues();
        if (returnToTitleButton != null)
        {
            returnToTitleButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("TitleScene");
            });
        }
    }

    [ContextMenu("Refresh With Test Values")]
    public void RefreshWithTestValues()
    {
        ScoreManager.ScoreResult result;
        if (ScoreRuntimeData.HasData)
        {
            result = ScoreRuntimeData.LastResult;
        }
        else
        {
            result = ScoreManager.CalculateScore(
                testRoundsCompleted,
                testSacrificedExcludingWitch,
                testCorrectMemoCount);
        }

        ApplyResult(result);
    }

    public void ApplyResult(ScoreManager.ScoreResult result)
    {
        StopAllCoroutines();
        SetReturnToTitleVisible(false);

        if (useSequentialReveal)
        {
            StartCoroutine(RevealResultRoutine(result));
            return;
        }

        ApplyResultImmediate(result);
    }

    private IEnumerator RevealResultRoutine(ScoreManager.ScoreResult result)
    {
        ClearAllTexts();

        bool hasSplitTexts =
            roundsLabelText != null || roundsValueText != null ||
            sacrificedLabelText != null || sacrificedValueText != null ||
            correctMemoLabelText != null || correctMemoValueText != null ||
            totalScoreLabelText != null || totalScoreValueText != null ||
            gradeLabelText != null || gradeValueText != null;

        if (hasSplitTexts)
        {
            SetRoundsLabel();
            yield return new WaitForSeconds(revealInterval);
            SetRoundsValue(result);
            yield return new WaitForSeconds(revealInterval);

            SetSacrificedLabel();
            yield return new WaitForSeconds(revealInterval);
            SetSacrificedValue(result);
            yield return new WaitForSeconds(revealInterval);

            SetCorrectMemoLabel();
            yield return new WaitForSeconds(revealInterval);
            SetCorrectMemoValue(result);
            yield return new WaitForSeconds(revealInterval);

            SetTotalScoreLabel();
            yield return new WaitForSeconds(revealInterval);
            SetTotalScoreValue(result);
            yield return new WaitForSeconds(revealInterval);

            SetGradeLabel();
            yield return new WaitForSeconds(revealInterval);
            SetGradeValue(result);
            yield return new WaitForSeconds(revealInterval);
            SetReturnToTitleVisible(true);
            yield break;
        }

        SetRoundsRow(result);
        yield return new WaitForSeconds(revealInterval);
        SetSacrificedRow(result);
        yield return new WaitForSeconds(revealInterval);
        SetCorrectMemoRow(result);
        yield return new WaitForSeconds(revealInterval);
        SetTotalScoreRow(result);
        yield return new WaitForSeconds(revealInterval);
        SetGradeRow(result);
        yield return new WaitForSeconds(revealInterval);
        SetReturnToTitleVisible(true);
    }

    private void ApplyResultImmediate(ScoreManager.ScoreResult result)
    {
        SetRoundsRow(result);
        SetSacrificedRow(result);
        SetCorrectMemoRow(result);
        SetTotalScoreRow(result);
        SetGradeRow(result);
        SetReturnToTitleVisible(true);
    }

    private void ClearAllTexts()
    {
        if (roundsText != null) roundsText.text = string.Empty;
        if (sacrificedText != null) sacrificedText.text = string.Empty;
        if (correctMemoText != null) correctMemoText.text = string.Empty;
        if (totalScoreText != null) totalScoreText.text = string.Empty;
        if (gradeText != null) gradeText.text = string.Empty;

        if (roundsLabelText != null) roundsLabelText.text = string.Empty;
        if (roundsValueText != null) roundsValueText.text = string.Empty;
        if (sacrificedLabelText != null) sacrificedLabelText.text = string.Empty;
        if (sacrificedValueText != null) sacrificedValueText.text = string.Empty;
        if (correctMemoLabelText != null) correctMemoLabelText.text = string.Empty;
        if (correctMemoValueText != null) correctMemoValueText.text = string.Empty;
        if (totalScoreLabelText != null) totalScoreLabelText.text = string.Empty;
        if (totalScoreValueText != null) totalScoreValueText.text = string.Empty;
        if (gradeLabelText != null) gradeLabelText.text = string.Empty;
        if (gradeValueText != null) gradeValueText.text = string.Empty;
    }



    private void SetRoundsRow(ScoreManager.ScoreResult result)
    {
        SetRoundsLabel();
        SetRoundsValue(result);
        if (roundsLabelText == null && roundsValueText == null && roundsText != null)
        {
            roundsText.text = $"진행된 라운드 수: {result.RoundsCompleted}";
        }
    }

    private void SetSacrificedRow(ScoreManager.ScoreResult result)
    {
        SetSacrificedLabel();
        SetSacrificedValue(result);
        if (sacrificedLabelText == null && sacrificedValueText == null && sacrificedText != null)
        {
            sacrificedText.text = $"희생된 사람의 수: {result.SacrificedCountExcludingWitch}";
        }
    }

    private void SetCorrectMemoRow(ScoreManager.ScoreResult result)
    {
        SetCorrectMemoLabel();
        SetCorrectMemoValue(result);
        if (correctMemoLabelText == null && correctMemoValueText == null && correctMemoText != null)
        {
            correctMemoText.text = $"올바르게 추리한 수: {result.CorrectMemoCount}";
        }
    }

    private void SetTotalScoreRow(ScoreManager.ScoreResult result)
    {
        SetTotalScoreLabel();
        SetTotalScoreValue(result);
        if (totalScoreLabelText == null && totalScoreValueText == null && totalScoreText != null)
        {
            totalScoreText.text = $"총점: {result.TotalScore}";
        }
    }

    private void SetGradeRow(ScoreManager.ScoreResult result)
    {
        SetGradeLabel();
        SetGradeValue(result);
        if (gradeLabelText == null && gradeValueText == null && gradeText != null)
        {
            gradeText.text = result.Grade;
        }
    }

    private void SetRoundsLabel()
    {
        if (roundsLabelText != null) roundsLabelText.text = "진행된 라운드 수:";
    }

    private void SetRoundsValue(ScoreManager.ScoreResult result)
    {
        if (roundsValueText != null) roundsValueText.text = result.RoundsCompleted.ToString();
    }

    private void SetSacrificedLabel()
    {
        if (sacrificedLabelText != null) sacrificedLabelText.text = "희생된 사람의 수:";
    }

    private void SetSacrificedValue(ScoreManager.ScoreResult result)
    {
        if (sacrificedValueText != null) sacrificedValueText.text = result.SacrificedCountExcludingWitch.ToString();
    }

    private void SetCorrectMemoLabel()
    {
        if (correctMemoLabelText != null) correctMemoLabelText.text = "올바르게 추리한 수:";
    }

    private void SetCorrectMemoValue(ScoreManager.ScoreResult result)
    {
        if (correctMemoValueText != null) correctMemoValueText.text = result.CorrectMemoCount.ToString();
    }

    private void SetTotalScoreLabel()
    {
        if (totalScoreLabelText != null) totalScoreLabelText.text = "총점:";
    }

    private void SetTotalScoreValue(ScoreManager.ScoreResult result)
    {
        if (totalScoreValueText != null) totalScoreValueText.text = result.TotalScore.ToString();
    }

    private void SetGradeLabel()
    {
        if (gradeLabelText != null) gradeLabelText.text = string.Empty;
    }

    private void SetGradeValue(ScoreManager.ScoreResult result)
    {
        if (gradeValueText != null) gradeValueText.text = result.Grade;
    }

    private void SetReturnToTitleVisible(bool visible)
    {
        if (returnToTitleButton == null) return;
        returnToTitleButton.gameObject.SetActive(visible);
    }
}
