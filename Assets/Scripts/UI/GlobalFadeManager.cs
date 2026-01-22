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
            Debug.LogWarning("Fade 이미지가 없습니다.");
        }
    }

    public void SetFocus(bool isFocused)
    {
        fadePanel.DOKill();

        float endAlpha = isFocused ? targetAlpha : 0.0f;

        fadePanel.DOFade(endAlpha, fadeDuration).SetUpdate(true);
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
