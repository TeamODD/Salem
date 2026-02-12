using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private Button _openButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _titleButton;
    [SerializeField] private Button _quitButton;

    private void Awake()
    {
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
    }

    private void Start()
    {
        if (_openButton != null) _openButton.onClick.AddListener(OpenSettings);
        if (_closeButton != null) _closeButton.onClick.AddListener(CloseSettings);
        if (_titleButton != null) _titleButton.onClick.AddListener(GotoTitle);
        if (_quitButton != null) _quitButton.onClick.AddListener(QuitGame);
    }
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_settingsPanel.activeSelf)
                CloseSettings();
            else
                OpenSettings();
        }
    }
    public void OpenSettings()
    {
        _settingsPanel.SetActive(true);
        SoundManager.Instance.PauseBGM();
        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        _settingsPanel.SetActive(false);
        SoundManager.Instance.ResumeBGM();
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
