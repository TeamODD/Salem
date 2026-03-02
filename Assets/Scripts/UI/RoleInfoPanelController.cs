using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System;

public class RoleInfoPanelController : MonoBehaviour
{
    [Header("데이터")]
    [Tooltip("역할 정보가 저장된 ScriptableObject")]
    [SerializeField] private RoleIntroData _roleIntroData;

    [Header("UI 참조")]
    [SerializeField] private Button _openButton;
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _roleNameText;
    [SerializeField] private Image _roleIconImage;
    [SerializeField] private TextMeshProUGUI _roleDescText;
    [SerializeField] private TextMeshProUGUI _pageIndicator;

    [Header("네비게이션 버튼")]
    [SerializeField] private Button _prevButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _closeButton;

    [Header("애니메이션")]
    [SerializeField] private float _openDuration = 0.3f;
    [SerializeField] private float _closeDuration = 0.2f;

    [Header("사운드")]
    [SerializeField] private AudioClip _openSound;
    [SerializeField] private AudioClip _closeSound;

    private List<RoleEntry> _roleList;
    private int _currentIndex;
    private bool _isOpen;

    public bool IsOpen => _isOpen;
    public Action OnPanelClosed;

    private void Awake()
    {
        if (_panel != null) _panel.SetActive(false);
    }

    private void Start()
    {
        // 데이터 로드
        if (_roleIntroData != null)
        {
            _roleList = _roleIntroData.AllRoles;
        }

        // 버튼 이벤트 등록
        if (_openButton != null) _openButton.onClick.AddListener(Open);
        if (_prevButton != null) _prevButton.onClick.AddListener(ShowPrev);
        if (_nextButton != null) _nextButton.onClick.AddListener(ShowNext);
        if (_closeButton != null) _closeButton.onClick.AddListener(Close);
    }
    
    public void Open()
    {
        if (_roleIntroData != null)
        {
            Open(_roleIntroData.AllRoles);
        }
    }

    public void Open(List<RoleEntry> roleList)
    {
        if (_isOpen) return;
        if (roleList == null || roleList.Count == 0)
        {
            return;
        }

        _roleList = roleList;

        if (_openSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(_openSound);
        }

        _isOpen = true;
        _currentIndex = 0;
    
        _panel.SetActive(true);
        UpdateDisplay();

        _panel.transform.localScale = Vector3.zero;
        _panel.transform.DOScale(Vector3.one, _openDuration).SetEase(Ease.OutBack);
    }

    public void Close()
    {
        if (!_isOpen) return;

        if (_closeSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(_closeSound);
        }

        _panel.transform.DOScale(Vector3.zero, _closeDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            _panel.SetActive(false);
            _isOpen = false;
            OnPanelClosed?.Invoke();
        });
    }

    private void ShowPrev()
    {
        if (!_isOpen || _currentIndex <= 0) return;

        _currentIndex--;
        UpdateDisplay();
    }

    private void ShowNext()
    {
        if (!_isOpen || _roleList == null) return;
        if (_currentIndex >= _roleList.Count - 1) return;

        _currentIndex++;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_roleList == null || _currentIndex < 0 || _currentIndex >= _roleList.Count)
            return;

        RoleEntry current = _roleList[_currentIndex];

        // 역할 이름
        if (_roleNameText != null) _roleNameText.text = current.RoleName;

        // 역할 설명
        if (_roleDescText != null) _roleDescText.text = current.RoleDescription;

        // 역할 아이콘
        if (_roleIconImage != null)
        {
            if (current.RoleIcon != null)
            {
                _roleIconImage.sprite = current.RoleIcon;
                _roleIconImage.gameObject.SetActive(true);
            }
            else
            {
                _roleIconImage.gameObject.SetActive(false);
            }
        }

        if (_pageIndicator != null)
        {
            _pageIndicator.text = $"{_currentIndex + 1}/{_roleList.Count}";
        }

        // 네비게이션 버튼 상태 갱신
        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        bool hasPrev = _currentIndex > 0;
        bool hasNext = _currentIndex < _roleList.Count - 1;

        if (_prevButton != null) _prevButton.gameObject.SetActive(hasPrev);
        if (_nextButton != null) _nextButton.gameObject.SetActive(hasNext);
    }

    private void OnDestroy()
    {
        if (_panel != null)
        {
            _panel.transform.DOKill();
        }
    }
}
