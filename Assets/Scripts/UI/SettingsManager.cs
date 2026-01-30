using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private CanvasGroup _mainCanvas;

    [SerializeField] private Button _openButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _titleButton;
    [SerializeField] private Button _quitButton;

    [SerializeField] private float _duration = 0.5f;

    private void Start()
    {
        if (_openButton != null) _openButton.onClick.AddListener(OpenSettings);
        if (_closeButton != null) _closeButton.onClick.AddListener(CloseSettings);
        if (_titleButton != null) _titleButton.onClick.AddListener(GotoTitle);
        if (_quitButton != null) _quitButton.onClick.AddListener(QuitGame);

        _settingsPanel.SetActive(false);
        _settingsPanel.transform.localScale = Vector3.zero;
        if (_mainCanvas != null) _mainCanvas.alpha = 0f;
    }

    public void OpenSettings()
    {
        _settingsPanel.SetActive(true);
        Time.timeScale = 0f;

        _settingsPanel.transform.DOScale(1f, _duration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);

        if (_mainCanvas != null)
            _mainCanvas.DOFade(1f, _duration).SetUpdate(true);
    }

    public void CloseSettings()
    {
        _settingsPanel.transform.DOScale(0f, _duration)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _settingsPanel.SetActive(false);
                Time.timeScale = 1f;
            });

        _mainCanvas.DOFade(0f, _duration).SetUpdate(true);
    }

    public void GotoTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
