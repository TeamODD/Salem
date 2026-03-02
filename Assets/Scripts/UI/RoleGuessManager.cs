using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

[Serializable]
public class RoleIconData
{
    public Role.Roles Role;
    public Sprite RoleIcon;
    public Button TargetButton;
}

public class RoleGuessManager : MonoBehaviour
{
    public static RoleGuessManager Instance;

    [SerializeField] private GameObject _selectorPanel;
    [SerializeField] private List<RoleIconData> _roleIcons;

    [Header("Default Icon Settings")]
    [SerializeField] private Button _defaultButton;
    [SerializeField] private Sprite _defaultSprite;

    [Header("Selector Position Settings")]
    [Tooltip("selector panel이 마크 위에 표시될 때의 Y 오프셋 (Screen Space 픽셀 단위)")]
    [SerializeField] private float _selectorYOffset = 200f;

    private CharacterMark _currentActiveMark;
    private List<CharacterMark> _allMarks = new List<CharacterMark>();
    private RectTransform _selectorCanvasRect;
    private Camera _mainCamera;

    void Awake()
    {
        if (Instance == null) Instance = this;
        _selectorPanel.SetActive(false);
    }

    void Start()
    {
        _mainCamera = Camera.main;

        // selectorPanel이 속한 Canvas의 RectTransform 캐싱
        Canvas selectorCanvas = _selectorPanel.GetComponentInParent<Canvas>();
        if (selectorCanvas != null)
        {
            _selectorCanvasRect = selectorCanvas.GetComponent<RectTransform>();
        }

        for (int i = 0; i < _roleIcons.Count; i++)
        {
            int index = i;
            _roleIcons[i].TargetButton.onClick.AddListener(() => OnSelectRole(index));
        }

        if (_defaultButton != null)
        {
            _defaultButton.onClick.AddListener(ResetToDefault);
        }
    }

    public void RegisterMark(CharacterMark mark)
    {
        if (!_allMarks.Contains(mark))
        {
            _allMarks.Add(mark);
            mark.SetGuessedRole(_defaultSprite);
        }
    }

    public void ResetAllMarksToDefault()
    {
        CloseSelector();
        _allMarks.RemoveAll(mark => mark == null);

        for (int i = 0; i < _allMarks.Count; i++)
        {
            _allMarks[i].SetGuessedRole(_defaultSprite);
        }
    }

    public void OpenSelector(CharacterMark mark)
    {
        if (_selectorPanel.activeSelf && _currentActiveMark == mark)
        {
            CloseSelector();
            return;
        }

        _currentActiveMark = mark;
        _selectorPanel.SetActive(true);

        // Mark의 월드 좌표를 Screen Space Canvas 좌표로 변환하여 패널 배치
        PositionSelectorAboveMark(mark);
    }

    public void ResetToDefault()
    {
        if (_currentActiveMark != null && _defaultSprite != null)
        {
            _currentActiveMark.SetGuessedRole(_defaultSprite);
        }

        CloseSelector();
    }

    public void OnSelectRole(int roleIndex)
    {
        Role.Roles selectedRole = (Role.Roles)roleIndex;
        Sprite icon = _roleIcons.Find(x => x.Role == selectedRole).RoleIcon;

        if (_currentActiveMark != null)
        {
            _currentActiveMark.SetGuessedRole(icon);
        }

        CloseSelector();
    }

    public void CloseSelector()
    {
        _selectorPanel.SetActive(false);
        _currentActiveMark = null;
    }

    /// <summary>
    /// Mark(World Space)의 월드 좌표를 Screen Space Canvas 좌표로 변환하여
    /// selectorPanel을 마크 위에 올바르게 배치합니다.
    /// </summary>
    private void PositionSelectorAboveMark(CharacterMark mark)
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_mainCamera == null || _selectorCanvasRect == null) return;

        // 1. Mark의 월드 좌표 → 스크린 좌표
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(mark.transform.position);

        // 2. 스크린 좌표에 Y 오프셋 추가 (스크린 픽셀 단위)
        screenPos.y += _selectorYOffset;

        // 3. 스크린 좌표 → Screen Space Canvas의 로컬 좌표
        RectTransform panelRect = _selectorPanel.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _selectorCanvasRect, screenPos, null, out localPoint);

        panelRect.anchoredPosition = localPoint;
    }
}
