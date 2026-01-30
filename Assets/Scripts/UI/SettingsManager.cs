using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject _settingsPanel;
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
    }

    public void OpenSettings()
    {
        _settingsPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        _settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void GotoTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
