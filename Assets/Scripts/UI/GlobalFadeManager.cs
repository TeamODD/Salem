using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class GlobalFadeManager : MonoBehaviour
{
    public static GlobalFadeManager Instance { get; private set; }

    [Header("Setting")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float targetAlpha = 0.6f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (fadePanel == null)
        {
            Debug.LogWarning("[GlobalFadeManager] Fade panel is not assigned.");
        }
    }

    public void SetFocus(bool isFocused)
    {
        if (fadePanel == null) return;

        fadePanel.DOKill();

        float endAlpha = isFocused ? targetAlpha : 0.0f;

        fadePanel.DOFade(endAlpha, fadeDuration).SetUpdate(true);
    }

    public void FadeFullOut(float duration, System.Action onComplete = null)
    {
        if (fadePanel == null)
        {
            onComplete?.Invoke();
            return;
        }

        fadePanel.DOKill();
        fadePanel.DOFade(1.0f, duration).SetUpdate(true).OnComplete(() => onComplete?.Invoke());
    }

    public void FadeFullIn(float duration, System.Action onComplete = null)
    {
        if (fadePanel == null)
        {
            onComplete?.Invoke();
            return;
        }

        fadePanel.DOKill();
        fadePanel.DOFade(0.0f, duration).SetUpdate(true).OnComplete(() => onComplete?.Invoke());
    }

    public void SetAlpha(float alpha)
    {
        if (fadePanel == null) return;

        fadePanel.DOKill();
        Color color = fadePanel.color;
        color.a = Mathf.Clamp01(alpha);
        fadePanel.color = color;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (fadePanel != null)
        {
            fadePanel.DOKill();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetData()
    {
        Instance = null;
    }
}
