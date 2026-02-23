using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    public static IntroManager Instance { get; private set; }
    public CanvasGroup PanelCanvasGroup;
    public TextMeshProUGUI IntroText;

    public float TypeSpeed = 0.05f;
    public float DisplayDuration = 2f;
    public float FadeDuration = 1f;

    public bool IsIntroPlaying { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PanelCanvasGroup == null || IntroText == null)
        {
            Debug.LogError("[IntroManager] PanelCanvasGroup 또는 IntroText가 설정되지 않았습니다.");
            enabled = false;
            return;
        }

        // GameManager에서 LoadLevel 시 호출하므로 여기서는 초기화만 수행
        PanelCanvasGroup.alpha = 0f;
        PanelCanvasGroup.blocksRaycasts = false;
        PanelCanvasGroup.gameObject.SetActive(false);
        IntroText.text = "";
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowIntro(string levelName, List<Role.Roles> assignedRoles)
    {
        if (!enabled) return;

        // 이전 트윈 중단 및 초기화
        IntroText.DOKill();
        PanelCanvasGroup.DOKill();

        IsIntroPlaying = true;
        PanelCanvasGroup.gameObject.SetActive(true);
        PanelCanvasGroup.alpha = 1f;
        PanelCanvasGroup.blocksRaycasts = true;
        IntroText.text = "";
        IntroText.alpha = 1f;

        // 중복 제거 및 "마녀"를 제외한 직업 목록 생성 (마녀는 어차피 있다고 명시하므로)
        HashSet<Role.Roles> uniqueRoles = new HashSet<Role.Roles>(assignedRoles);
        uniqueRoles.Remove(Role.Roles.마녀);

        string rolesStr = string.Join(", ", uniqueRoles);
        string infoText = $"{levelName}\n\n이 마을에는 {rolesStr}이(가) 살고 있다.\n\n<color=red>이들 중에는 1명의 마녀가 숨어있다.</color>";

        PlayIntroSequence(infoText);
    }

    public void ShowGameOver(string message)
    {
        if (!enabled) return;

        IsIntroPlaying = true; // 입력을 막기 위해 true로 설정
        IntroText.DOKill();
        PanelCanvasGroup.DOKill();

        PanelCanvasGroup.gameObject.SetActive(true);
        PanelCanvasGroup.alpha = 1f;
        PanelCanvasGroup.blocksRaycasts = true;
        IntroText.text = message;
        IntroText.alpha = 0f;

        if (GlobalFadeManager.Instance != null)
        {
            GlobalFadeManager.Instance.FadeFullOut(FadeDuration);
        }

        IntroText.DOFade(1f, FadeDuration);

        DOVirtual.DelayedCall(2f, () =>
        {
            SceneManager.LoadScene("TitleScene");
        });
    }

    private void PlayIntroSequence(string infoText)
    {
        int totalLength = infoText.Length;

        Sequence introSequence = DOTween.Sequence();

        int visibleCount = 0;

        introSequence.Append(
            DOTween.To(
                () => visibleCount,
                x =>
                {
                    visibleCount = x;
                    IntroText.text = infoText.Substring(0, visibleCount);
                },
                totalLength,
                totalLength * TypeSpeed
            ).SetEase(Ease.Linear)
        );

        introSequence.AppendInterval(DisplayDuration);

        introSequence.Append(IntroText.DOFade(0f, FadeDuration));

        introSequence.OnComplete(() =>
        {
            IsIntroPlaying = false;
            PanelCanvasGroup.alpha = 0f;
            PanelCanvasGroup.blocksRaycasts = false;
            PanelCanvasGroup.gameObject.SetActive(false);
        });
    }
}
