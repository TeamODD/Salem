using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    public static IntroManager Instance { get; private set; }
    public CanvasGroup PanelCanvasGroup;
    public TextMeshProUGUI IntroText;
    [SerializeField] private VideoClip defeatVideoClip;
    [SerializeField] private string defeatVideoResourcePath = "Animations/DeadScene/마녀 죽음 최종본5";

    public float TypeSpeed = 0.05f;
    public float DisplayDuration = 2f;
    public float FadeDuration = 1f;
    public float WarningRevealDelay = 0.4f;
    public float WarningFadeDuration = 0.35f;

    public bool IsIntroPlaying { get; private set; }

    private Coroutine _gameOverRoutine;
    private VideoPlayer _defeatVideoPlayer;
    private RawImage _defeatVideoImage;
    private RenderTexture _defeatRenderTexture;
    private Color _introTextBaseColor;
    private bool _hasStoredOriginalBgmVolume;
    private float _originalBgmVolume;

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
        _introTextBaseColor = IntroText.color;
        _hasStoredOriginalBgmVolume = false;
        _originalBgmVolume = 0f;
    }

    private void OnDestroy()
    {
        StopGameOverVideo();

        if (_defeatRenderTexture != null)
        {
            _defeatRenderTexture.Release();
            Destroy(_defeatRenderTexture);
            _defeatRenderTexture = null;
        }

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
        SetIntroTextAlpha(1f);

        // 중복 제거 및 "마녀"를 제외한 직업 목록 생성 (마녀는 어차피 있다고 명시하므로)
        HashSet<Role.Roles> uniqueRoles = new HashSet<Role.Roles>(assignedRoles);
        uniqueRoles.Remove(Role.Roles.마녀);

        string rolesStr = string.Join(", ", uniqueRoles);
        string mainText = $"{levelName}\n\n이 마을에는 {rolesStr}이(가) 살고 있다.";
        string witchWarningText = "\n\n이들 중에는 1명의 마녀가 숨어있다.";

        PlayIntroSequence(mainText, witchWarningText);
    }

    public void ShowGameOver(string message)
    {
        if (!enabled) return;

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        IntroText.DOKill();
        PanelCanvasGroup.DOKill();

        if (_gameOverRoutine != null)
        {
            StopCoroutine(_gameOverRoutine);
        }

        _gameOverRoutine = StartCoroutine(PlayGameOverSequence(message));
    }

    public void ShowNightDeaths(IReadOnlyList<string> deadNames, float displayDuration)
    {
        if (!enabled) return;

        IsIntroPlaying = true;
        IntroText.DOKill();
        PanelCanvasGroup.DOKill();

        PanelCanvasGroup.gameObject.SetActive(true);
        PanelCanvasGroup.alpha = 1f;
        PanelCanvasGroup.blocksRaycasts = true;

        IntroText.text = BuildNightDeathMessage(deadNames);
        SetIntroTextAlpha(0f);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(IntroText.DOFade(1f, FadeDuration));
        sequence.AppendInterval(displayDuration);
        sequence.Append(IntroText.DOFade(0f, FadeDuration));
        sequence.OnComplete(() =>
        {
            IsIntroPlaying = false;
            PanelCanvasGroup.alpha = 0f;
            PanelCanvasGroup.blocksRaycasts = false;
            PanelCanvasGroup.gameObject.SetActive(false);
            RestoreIntroVisualState();
        });
    }

    private void PlayIntroSequence(string mainText, string witchWarningText)
    {
        int totalLength = mainText.Length;

        Sequence introSequence = DOTween.Sequence();

        int visibleCount = 0;

        introSequence.Append(
            DOTween.To(
                () => visibleCount,
                x =>
                {
                    visibleCount = x;
                    IntroText.text = mainText.Substring(0, visibleCount);
                },
                totalLength,
                totalLength * TypeSpeed
            ).SetEase(Ease.Linear)
        );

        introSequence.AppendInterval(WarningRevealDelay);

        int warningAlpha = 0;
        introSequence.AppendCallback(() =>
        {
            warningAlpha = 0;
            IntroText.text = BuildIntroTextWithWarningAlpha(mainText, witchWarningText, warningAlpha);
        });

        introSequence.Append(
            DOTween.To(
                () => warningAlpha,
                x =>
                {
                    warningAlpha = x;
                    IntroText.text = BuildIntroTextWithWarningAlpha(mainText, witchWarningText, warningAlpha);
                },
                255,
                WarningFadeDuration
            ).SetEase(Ease.OutQuad)
        );

        introSequence.AppendInterval(DisplayDuration);

        introSequence.Append(IntroText.DOFade(0f, FadeDuration));

        introSequence.OnComplete(() =>
        {
            IsIntroPlaying = false;
            PanelCanvasGroup.alpha = 0f;
            PanelCanvasGroup.blocksRaycasts = false;
            PanelCanvasGroup.gameObject.SetActive(false);
            RestoreIntroVisualState();
        });
    }

    private System.Collections.IEnumerator PlayGameOverSequence(string message)
    {
        IsIntroPlaying = true;
        PanelCanvasGroup.gameObject.SetActive(true);
        PanelCanvasGroup.alpha = 1f;
        PanelCanvasGroup.blocksRaycasts = true;
        IntroText.text = message;
        SetIntroTextAlpha(0f);

        if (GlobalFadeManager.Instance != null)
        {
            GlobalFadeManager.Instance.FadeFullOut(FadeDuration);
        }

        if (TryGetDefeatVideoClip(out VideoClip clip) && TryEnsureDefeatVideoPlayer())
        {
            MuteBgmForDefeatVideo();
            _defeatVideoPlayer.Stop();
            _defeatVideoPlayer.clip = clip;
            _defeatVideoPlayer.Prepare();
            yield return new WaitUntil(() => _defeatVideoPlayer.isPrepared);

            _defeatVideoImage.gameObject.SetActive(true);
            _defeatVideoImage.transform.SetAsLastSibling();
            _defeatVideoPlayer.Play();
            yield return new WaitUntil(() => !_defeatVideoPlayer.isPlaying);

            StopGameOverVideo();
        }
        else
        {
            IntroText.DOFade(1f, FadeDuration);
            yield return new WaitForSeconds(2f);
        }

        IsIntroPlaying = false;
        RestoreIntroVisualState();
        _gameOverRoutine = null;
    }

    private bool TryGetDefeatVideoClip(out VideoClip clip)
    {
        clip = defeatVideoClip;
        if (clip != null) return true;

        if (string.IsNullOrWhiteSpace(defeatVideoResourcePath)) return false;

        clip = Resources.Load<VideoClip>(defeatVideoResourcePath);
        defeatVideoClip = clip;
        return clip != null;
    }

    private bool TryEnsureDefeatVideoPlayer()
    {
        EnsureDefeatVideoImage();
        EnsureDefeatRenderTexture();

        _defeatVideoPlayer = gameObject.GetComponent<VideoPlayer>();
        if (_defeatVideoPlayer == null)
        {
            _defeatVideoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        _defeatVideoPlayer.playOnAwake = false;
        _defeatVideoPlayer.isLooping = false;
        _defeatVideoPlayer.skipOnDrop = true;
        _defeatVideoPlayer.waitForFirstFrame = true;
        _defeatVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _defeatVideoPlayer.targetTexture = _defeatRenderTexture;
        _defeatVideoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        _defeatVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        _defeatVideoPlayer.SetDirectAudioMute(0, false);
        _defeatVideoPlayer.SetDirectAudioVolume(0, 1f);
        return true;
    }

    private void StopGameOverVideo()
    {
        if (_defeatVideoPlayer != null)
        {
            _defeatVideoPlayer.Stop();
            _defeatVideoPlayer.clip = null;
            _defeatVideoPlayer.targetTexture = _defeatRenderTexture;
        }

        if (_defeatVideoImage != null)
        {
            _defeatVideoImage.gameObject.SetActive(false);
        }

        if (_defeatRenderTexture != null)
        {
            RenderTexture activeTexture = RenderTexture.active;
            RenderTexture.active = _defeatRenderTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = activeTexture;
        }

        RestoreBgmAfterDefeatVideo();
    }

    private void EnsureDefeatVideoImage()
    {
        if (_defeatVideoImage != null) return;

        GameObject videoObject = new GameObject("DefeatVideo", typeof(RectTransform), typeof(RawImage));
        videoObject.transform.SetParent(PanelCanvasGroup.transform, false);

        RectTransform rectTransform = videoObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.SetAsLastSibling();

        _defeatVideoImage = videoObject.GetComponent<RawImage>();
        _defeatVideoImage.color = Color.white;
        _defeatVideoImage.raycastTarget = false;
        _defeatVideoImage.gameObject.SetActive(false);
    }

    private void EnsureDefeatRenderTexture()
    {
        int width = Mathf.Max(Screen.width, 1920);
        int height = Mathf.Max(Screen.height, 1080);

        if (_defeatRenderTexture != null &&
            _defeatRenderTexture.width == width &&
            _defeatRenderTexture.height == height)
        {
            return;
        }

        if (_defeatRenderTexture != null)
        {
            _defeatRenderTexture.Release();
            Destroy(_defeatRenderTexture);
        }

        _defeatRenderTexture = new RenderTexture(width, height, 0)
        {
            name = "DefeatVideoRenderTexture"
        };
        _defeatRenderTexture.Create();

        if (_defeatVideoImage != null)
        {
            _defeatVideoImage.texture = _defeatRenderTexture;
        }
    }

    private string BuildNightDeathMessage(IReadOnlyList<string> deadNames)
    {
        if (deadNames == null || deadNames.Count == 0)
        {
            return "밤사이 아무도 죽지 않았습니다.";
        }

        if (deadNames.Count == 1)
        {
            return $"{deadNames[0]}이(가) 죽었습니다.";
        }

        return $"밤사이 {deadNames.Count}명이 죽었습니다.\n{string.Join(", ", deadNames)}";
    }

    private string BuildIntroTextWithWarningAlpha(string mainText, string witchWarningText, int alpha)
    {
        string alphaHex = Mathf.Clamp(alpha, 0, 255).ToString("X2");
        return $"{mainText}<color=#FF0000{alphaHex}>{witchWarningText}</color>";
    }

    private void RestoreIntroVisualState()
    {
        PanelCanvasGroup.alpha = 0f;
        PanelCanvasGroup.blocksRaycasts = false;
        SetIntroTextAlpha(1f);
    }

    private void SetIntroTextAlpha(float alpha)
    {
        if (IntroText == null) return;

        Color color = _introTextBaseColor;
        color.a = Mathf.Clamp01(alpha);
        IntroText.color = color;
    }

    private void MuteBgmForDefeatVideo()
    {
        if (SoundManager.Instance == null || _hasStoredOriginalBgmVolume) return;

        _originalBgmVolume = SoundManager.Instance.GetBGMVolume();
        _hasStoredOriginalBgmVolume = true;
        SoundManager.Instance.SetBGMVolume(0f);
    }

    private void RestoreBgmAfterDefeatVideo()
    {
        if (SoundManager.Instance == null || !_hasStoredOriginalBgmVolume) return;

        SoundManager.Instance.SetBGMVolume(_originalBgmVolume);
        _hasStoredOriginalBgmVolume = false;
    }
}
