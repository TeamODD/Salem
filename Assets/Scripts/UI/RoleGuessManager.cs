using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

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
            if (_roleIcons[i].TargetButton != null)
            {
                int index = i;
                _roleIcons[i].TargetButton.onClick.AddListener(() => OnSelectRole(index));
            }

            if (_defaultButton != null)
            {
                _defaultButton.onClick.AddListener(ResetToDefault);
            }
        }
    }

    void Update()
    {
        if (_selectorPanel.activeSelf && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 셀렉터 패널 내부 또는 마크 이미지 위가 아닐 때만 닫기
            if (!IsPointerOverSelector() && !IsPointerOverMark())
            {
                CloseSelector();
            }
        }
    }

    public void RegisterMark(CharacterMark mark)
    {
        if (!_allMarks.Contains(mark))
        {
            _allMarks.Add(mark);
            mark.SetGuessedRole(null, _defaultSprite);
        }
    }

    public void ResetAllMarksToDefault()
    {
        CloseSelector();
        _allMarks.RemoveAll(mark => mark == null);

        for (int i = 0; i < _allMarks.Count; i++)
        {
            _allMarks[i].SetGuessedRole(null, _defaultSprite);
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
            _currentActiveMark.SetGuessedRole(null, _defaultSprite);
        }

        CloseSelector();
    }

    public void OnSelectRole(int roleIndex)
    {
        Role.Roles selectedRole = (Role.Roles)roleIndex;
        Sprite icon = _roleIcons.Find(x => x.Role == selectedRole).RoleIcon;

        if (_currentActiveMark != null)
        {
            _currentActiveMark.SetGuessedRole(selectedRole, icon);
        }

        CloseSelector();
    }

    public void CloseSelector()
    {
        _selectorPanel.SetActive(false);
        _currentActiveMark = null;
    }

    private bool IsPointerOverSelector()
    {
        RectTransform panelRect = _selectorPanel.GetComponent<RectTransform>();
        if (panelRect == null) return false;

        return RectTransformUtility.RectangleContainsScreenPoint(panelRect, Mouse.current.position.ReadValue());
    }

    private bool IsPointerOverMark()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);
        
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<CharacterMark>() != null)
            {
                return true;
            }
        }
        
        return false;
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

    public int CountCorrectGuesses()
    {
        _allMarks.RemoveAll(mark => mark == null);

        int correctCount = 0;
        for (int i = 0; i < _allMarks.Count; i++)
        {
            CharacterMark mark = _allMarks[i];
            if (!mark.GuessedRole.HasValue) continue;

            CharacterAI ai = mark.GetAttachedCharacterAI();
            if (ai == null) continue;

            if (mark.GuessedRole.Value == ai.MyRole)
            {
                correctCount++;
            }
        }

        return correctCount;
    }
}
