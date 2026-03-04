using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class TitleSceneController : MonoBehaviour
{
    [Header("UI")]
    public SpriteRenderer TitleRenderer;
    public Button GameStartButton;
    public CanvasGroup OtherTextsGroup; // 튜토리얼, 환경설정, 크레딧 등 하단 텍스트 그룹
    public Image BlackScreen; // 화면이 꺼질 때 사용할 검은색 전체 화면 이미지

    [Header("오브젝트")]
    public SpriteRenderer WomanBeforeRenderer;
    public SpriteRenderer WomanAfterRenderer;
    public SpriteRenderer FireBackRenderer;
    public SpriteRenderer FireFrontRenderer;
    public SpriteRenderer BackgroundMoonRenderer;

    [System.Serializable]
    public struct ToolAnimationData
    {
        public SpriteRenderer ToolRenderer;
        public Vector3 StartLocalPos;
        public Vector3 EndLocalPos;
    }

    [Header("농기구 개별 설정")]
    public ToolAnimationData[] ToolAnimations;

    [Header("애니메이션 설정")]
    public float AnimationDuration = 5f;
    public float WaitTimeAfterAnimation = 1f;
    public float BlackScreenDuration = 3f;

    [Header("타겟 값 (배율)")]
    public Color TitleTargetColor = Color.red;
    public float WomanScaleMultiplier = 0.5f;
    public float MoonScaleMultiplier = 1.5f;
    public float TargetBgmVolume = 1f;

    [Header("오디오")]
    public AudioClip TitleBgmClip; // 게임 시작 버튼을 누르면 서서히 커질 BGM
    public AudioClip ThumpSfxClip; // 화면이 꺼질 때 재생할 쿵 소리
    
    [Header("씬 전환")]
    public string NextSceneName = "MainScene";

    private void Start()
    {
        if (TitleRenderer != null)
            TitleRenderer.color = Color.white;

        // 초기 투명도 0 설정
        SetAlpha(WomanAfterRenderer, 0f);
        SetAlpha(FireBackRenderer, 0f);
        SetAlpha(FireFrontRenderer, 0f);

        GameStartButton.onClick.AddListener(OnGameStartPressed);
    }

    private void SetAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    private void OnGameStartPressed()
    {
        GameStartButton.interactable = false;

        if (SoundManager.Instance != null && TitleBgmClip != null)
        {
            SoundManager.Instance.SetBGMVolume(0f);
            SoundManager.Instance.PlayBGM(TitleBgmClip);
        }

        Sequence seq = DOTween.Sequence();
        // 캐릭터 및 불 페이드인
        if (WomanAfterRenderer != null)
            seq.Join(WomanAfterRenderer.DOFade(1f, AnimationDuration));

        if (FireBackRenderer != null && FireFrontRenderer != null)
        {
            seq.Join(FireBackRenderer.DOFade(1f, AnimationDuration));
            seq.Join(FireFrontRenderer.DOFade(1f, AnimationDuration));
        }
        // 1. 타이틀 글자 하얀색에서 빨간색으로 페이드인
        if (TitleRenderer != null)
            seq.Join(TitleRenderer.DOColor(TitleTargetColor, AnimationDuration));

        // 2. 다른 하단 UI 텍스트들은 페이드아웃 처리
        if (OtherTextsGroup != null)
            seq.Join(OtherTextsGroup.DOFade(0f, AnimationDuration * 0.2f));

        // 3. 여인 크기 작아짐 (기존 스케일 기준 곱연산)
        if (WomanBeforeRenderer != null)
            seq.Join(WomanBeforeRenderer.transform.DOScale(WomanBeforeRenderer.transform.localScale * WomanScaleMultiplier, AnimationDuration));
        if (WomanAfterRenderer != null)
            seq.Join(WomanAfterRenderer.transform.DOScale(WomanAfterRenderer.transform.localScale * WomanScaleMultiplier, AnimationDuration));

        // 4. 불 스케일 조정 (여인에 맞춰)
        if (FireBackRenderer != null && FireFrontRenderer != null)
        {
            seq.Join(FireBackRenderer.transform.DOScale(FireBackRenderer.transform.localScale * WomanScaleMultiplier, AnimationDuration));
            seq.Join(FireFrontRenderer.transform.DOScale(FireFrontRenderer.transform.localScale * WomanScaleMultiplier, AnimationDuration));
        }

        // 5. 반대로 배경의 달은 점점 커짐
        if (BackgroundMoonRenderer != null)
            seq.Join(BackgroundMoonRenderer.transform.DOScale(BackgroundMoonRenderer.transform.localScale * MoonScaleMultiplier, AnimationDuration));

        // 6. BGM 볼륨 점점 커짐
        if (SoundManager.Instance != null)
        {
            seq.Join(DOTween.To(() => SoundManager.Instance.GetBGMVolume(), 
                                x => SoundManager.Instance.SetBGMVolume(x), 
                                TargetBgmVolume, AnimationDuration).SetEase(Ease.Linear));
        }

        // 7. 애니메이션 완료 후 대기 및 씬 전환 코루틴 실행
        seq.OnComplete(() =>
        {
            foreach (var tool in ToolAnimations)
            {
                if (tool.ToolRenderer == null) continue;
                Transform child = tool.ToolRenderer.transform;

                child.localPosition = tool.StartLocalPos;
                child.DOLocalMove(tool.EndLocalPos, AnimationDuration * 0.2f)
                    .SetEase(Ease.OutCubic);
            }
            
            StartCoroutine(EndSequenceRoutine());
        });
    }

    private IEnumerator EndSequenceRoutine()
    {
        // 상황에서 잠시 기다림
        yield return new WaitForSeconds(WaitTimeAfterAnimation);

        // 쿵 효과음
        if (SoundManager.Instance != null && ThumpSfxClip != null)
        {
            SoundManager.Instance.PlaySFX(ThumpSfxClip);
        }
        
        // 단번에 화면이 꺼짐 (블랙스크린 표출)
        if (BlackScreen != null)
        {
            BlackScreen.color = Color.black;
            BlackScreen.gameObject.SetActive(true);
        }

        // 화면 꺼진 뒤의 극적인 잠시 대기
        yield return new WaitForSeconds(BlackScreenDuration);

        // 게임 시작 (씬 전환)
        SceneManager.LoadScene(NextSceneName);
    }
}
