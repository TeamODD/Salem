using UnityEngine;
using TMPro;
using DG.Tweening;

public class IntroManager : MonoBehaviour
{
    public static IntroManager Instance;
    public CanvasGroup PanelCanvasGroup;
    public TextMeshProUGUI IntroText;

    public float TypeSpeed = 0.05f;
    public float DisplayDuration = 2f;
    public float FadeDuration = 1f;

    public bool IsIntroPlaying { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        IsIntroPlaying = true;
        PanelCanvasGroup.alpha = 1f;
        PanelCanvasGroup.blocksRaycasts = true;
        IntroText.text = "";

        PlayIntro();
    }

    void PlayIntro()
    {
        string infoText = "마을에는 신자, 좀도둑, 겁쟁이가 살고 있다.\n\n" + "이들 중에는 1명의 마녀가 숨어있다.";
        int totalLength = infoText.Length;
        
        Sequence introSequence = DOTween.Sequence();

        int visibleCount = 0; // 1. 변화의 대상이 될 변수

        introSequence.Append( // 2. 시퀀스에 이 동작을 추가함
            DOTween.To(
                () => visibleCount,         // (A) Getter: 시작값을 어디서 가져올지
                x => {                      // (B) Setter: 값이 변할 때마다 무엇을 할지
                    visibleCount = x;
                    IntroText.text = infoText.Substring(0, visibleCount);
                }, 
                totalLength,                // (C) Target: 최종적으로 도달할 값
                totalLength * TypeSpeed     // (D) Duration: 애니메이션 소요 시간
            ).SetEase(Ease.Linear)          // 3. 일정한 속도로 진행
        );

        introSequence.AppendInterval(DisplayDuration);

        introSequence.Append(IntroText.DOFade(0f, FadeDuration));
        introSequence.Join(PanelCanvasGroup.DOFade(0f, FadeDuration));

        introSequence.OnComplete(() => {
            IsIntroPlaying = false;
            PanelCanvasGroup.gameObject.SetActive(false);
        });
    }
}