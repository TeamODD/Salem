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

    private CharacterMark _currentActiveMark;
    private List<CharacterMark> _allMarks = new List<CharacterMark>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        _selectorPanel.SetActive(false);
    }

    void Start()
    {
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
    public void OpenSelector(CharacterMark mark)
    {
        if (_selectorPanel.activeSelf && _currentActiveMark == mark)
        {
            CloseSelector();
            return;
        }
            
        _currentActiveMark = mark;
        _selectorPanel.SetActive(true);

        _selectorPanel.transform.position = mark.transform.position + new Vector3(0, 200, 0); // 마크 위에 표시

        //if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(null);
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
}