using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;

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
    private RectTransform _selectorPanelRect;
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

        // selectorPanel의 RectTransform 캐싱
        _selectorPanelRect = _selectorPanel.GetComponent<RectTransform>();

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
        SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
        if (_selectorPanel.activeSelf && _currentActiveMark == mark)
        {
            CloseSelector();
            return;
        }
        _currentActiveMark = mark;
        _selectorPanel.SetActive(true);

        AnimatePanelToPosition(mark);
    }

    public void ResetToDefault()
    {
        if (_currentActiveMark != null && _defaultSprite != null)
        {
            SoundManager.Instance.StopSFX();
            SoundManager.Instance.PlaySFX(SFXType.Memo);
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
            SoundManager.Instance.StopSFX();
            SoundManager.Instance.PlaySFX(SFXType.Memo);
            _currentActiveMark.SetGuessedRole(icon);
        }

        CloseSelector();
    }

    public void CloseSelector()
    {
        if (!_selectorPanel.activeSelf) return;

        _selectorPanelRect.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InBack).OnComplete(() =>
        {
            _selectorPanel.SetActive(false);
            _currentActiveMark = null;
        });
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

    /*
    private void PositionSelectorAboveMark(CharacterMark mark)
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_mainCamera == null || _selectorCanvasRect == null) return;

        // 1. Mark의 월드 좌표 → 스크린 좌표
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(mark.transform.position);

        // 2. 스크린 좌표에 Y 오프셋 추가 (스크린 픽셀 단위)
        screenPos.y += _selectorYOffset;

        // 3. 화면 경계 내로 클램핑 (스크린 좌표) – 기본적인 제한
        screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
        screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);

        // 4. 스크린 좌표 → Screen Space Canvas의 로컬 좌표
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _selectorCanvasRect, screenPos, null, out localPoint);

        // 5. 캔버스 크기와 패널 크기를 고려하여 로컬 좌표를 추가로 클램핑
        if (_selectorCanvasRect != null && _selectorPanelRect != null)
        {
            Vector2 canvasSize = _selectorCanvasRect.rect.size;
            Vector2 panelSize = _selectorPanelRect.rect.size;

            float halfCanvasWidth = canvasSize.x * 0.5f;
            float halfCanvasHeight = canvasSize.y * 0.5f;
            float halfPanelWidth = panelSize.x * 0.5f;
            float halfPanelHeight = panelSize.y * 0.5f;

            localPoint.x = Mathf.Clamp(localPoint.x,
                -halfCanvasWidth + halfPanelWidth,
                 halfCanvasWidth - halfPanelWidth);
            localPoint.y = Mathf.Clamp(localPoint.y,
                -halfCanvasHeight + halfPanelHeight,
                 halfCanvasHeight - halfPanelHeight);
        }

        _selectorPanelRect.anchoredPosition = localPoint;
    }
    */
    
    private void AnimatePanelToPosition(CharacterMark mark)
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_mainCamera == null || _selectorCanvasRect == null || _selectorPanelRect == null) return;

        // 최종 위치 계산 (PositionSelectorAboveMark와 동일 로직)
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(mark.transform.position);
        screenPos.y += _selectorYOffset;
        screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
        screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);

        Vector2 finalLocalPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _selectorCanvasRect, screenPos, null, out finalLocalPoint);

        // 캔버스 크기 고려 클램핑
        Vector2 canvasSize = _selectorCanvasRect.rect.size;
        Vector2 panelSize = _selectorPanelRect.rect.size;
        float halfCanvasWidth = canvasSize.x * 0.5f;
        float halfCanvasHeight = canvasSize.y * 0.5f;
        float halfPanelWidth = panelSize.x * 0.5f;
        float halfPanelHeight = panelSize.y * 0.5f;
        finalLocalPoint.x = Mathf.Clamp(finalLocalPoint.x, -halfCanvasWidth + halfPanelWidth, halfCanvasWidth - halfPanelWidth);
        finalLocalPoint.y = Mathf.Clamp(finalLocalPoint.y, -halfCanvasHeight + halfPanelHeight, halfCanvasHeight - halfPanelHeight);

        // 초기 위치: 최종 Y에서 오프셋만큼 아래로 (펼쳐지는 효과)
        Canvas canvas = _selectorCanvasRect.GetComponent<Canvas>();
        float scaleFactor = canvas != null ? canvas.scaleFactor : 1f;
        float localOffsetY = _selectorYOffset / scaleFactor;
        Vector2 initialLocalPoint = new Vector2(finalLocalPoint.x, finalLocalPoint.y - localOffsetY);

        // 초기 상태 설정
        _selectorPanelRect.anchoredPosition = initialLocalPoint;
        _selectorPanelRect.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(_selectorPanelRect.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
        seq.Join(_selectorPanelRect.DOAnchorPos(finalLocalPoint, 0.2f).SetEase(Ease.OutBack));
    }
}
