using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using Yarn.Unity;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("스테이지 목록")]
    public List<TutorialStage> Stages = new List<TutorialStage>();
    private TutorialStage _currentStage;
    
    [Header("UI 참조")]
    public RectTransform DialogueBox;
    public TextMeshProUGUI DescText;
    public Button NextButton; // 대화형 단계용 버튼
    public UIHighlightController UIHighlight;
    public Image Blocker; // 전체 화면 레이캐스트 차단막

    [Header("스테이지 인트로 UI (페이드 인/아웃)")]
    public CanvasGroup IntroPanel;
    public TextMeshProUGUI IntroRoundText;
    public TextMeshProUGUI IntroSituationText;
    public float IntroFadeDuration = 0.5f;
    public float IntroDisplayDuration = 2.0f;

    [Header("스킵 UI (검은 화면 및 처형자 표시)")]
    public CanvasGroup SkipBlackScreen;
    public TextMeshProUGUI ExecutedCharacterNameText;
    public float SkipScreenDuration = 2.0f; // 검은 화면 표시 시간
    public float ExecutedCharacterNameDisplayDuration = 1.5f; // 캐릭터 이름 표시 시간

    [Header("역할 설명 UI (팝업)")]
    public RoleInfoPanelController RoleInfoPanel;
    public Button RoleInfoOpenButton;

    [Header("시나리오 모드 참조")]
    [Tooltip("씬의 캐릭터 오브젝트들 (인덱스 순서대로)")]
    public List<CharacterAI> ScenarioCharacters = new List<CharacterAI>();
    
    [Tooltip("Yarn DialogueRunner")]
    public DialogueRunner DialogueRunner;
    
    [Tooltip("넘기기 버튼")]
    public Button SkipButton;
    
    [Tooltip("장전 버튼")]
    public Button LoadGunButton;

    [Header("설정")]
    public float HorizontalOffset = 250f;
    public float VerticalOffset = 0f;
    public float MoveDuration = 0.4f;

    private int _currentStageIndex = -1;
    private int _currentStepIndex = -1;

    private bool _isShowingIntro = false;
    private bool _isShowingRolePopup = false;
    private bool _wasBlockerActive;

    private TutorialRaycastFilter _blockerFilter;

    // 시나리오 핸들러 (위임)
    private TutorialScenarioHandler _scenarioHandler;

    void Awake()
    {
        Instance = this;
        if (Blocker != null) 
        {
            Blocker.gameObject.SetActive(false);
            // Blocker에 필터 컴포넌트 추가 확인
            _blockerFilter = Blocker.GetComponent<TutorialRaycastFilter>();
            if (_blockerFilter == null) _blockerFilter = Blocker.gameObject.AddComponent<TutorialRaycastFilter>();
        }
        if (NextButton != null) NextButton.onClick.AddListener(OnNextButtonClicked);
        if (RoleInfoOpenButton != null) RoleInfoOpenButton.onClick.AddListener(ShowAllRoles);
        if (SkipButton != null) SkipButton.onClick.AddListener(OnSkipButtonClicked);
        
        // 초기 상태 설정
        if (DialogueBox != null) DialogueBox.gameObject.SetActive(false);
        if (IntroPanel != null) { IntroPanel.alpha = 0; IntroPanel.gameObject.SetActive(false); }

        // 시나리오 핸들러 초기화
        _scenarioHandler = gameObject.GetComponent<TutorialScenarioHandler>();
        if (_scenarioHandler == null) _scenarioHandler = gameObject.AddComponent<TutorialScenarioHandler>();
        _scenarioHandler.Setup(
            ScenarioCharacters, DialogueRunner, DialogueBox, DescText,
            NextButton, UIHighlight, Blocker, _blockerFilter);
    }

    void Start()
    {
        if (DialogueRunner != null)
        {
            DialogueRunner.onNodeComplete.AddListener(OnDialogueNodeComplete);
        }

        // 필요 시 테스트를 위해 자동 시작하거나, 외부 호출 대기
        PlayStage(0); 
    }

    public void PlayStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= Stages.Count)
        {
            Debug.LogWarning($"[TutorialManager] 잘못된 스테이지 인덱스: {stageIndex}");
            return;
        }

        // 이전 스테이지의 모든 잔여 상태를 초기화
        ResetAllState();

        _currentStageIndex = stageIndex;
        _currentStage = Stages[_currentStageIndex];
        _currentStepIndex = 0;
        
        // 시나리오 모드 초기화 (시나리오 데이터가 있는 경우)
        _scenarioHandler.Initialize(_currentStage);
        
        _currentStage.OnStageStart?.Invoke();
        
        // 인트로 시퀀스 시작
        StartCoroutine(PlayStageIntroSequence());
    }

    /// <summary>
    /// 스테이지 전환 시 전체 상태 초기화 (새 게임 시작과 동일한 상태로)
    /// </summary>
    private void ResetAllState()
    {
        // 1. 진행 중인 코루틴 정지
        StopAllCoroutines();

        // 2. UI 초기화
        if (DialogueBox != null) DialogueBox.gameObject.SetActive(false);
        if (Blocker != null) Blocker.gameObject.SetActive(false);
        if (_blockerFilter != null) _blockerFilter.Clear();
        if (NextButton != null) NextButton.gameObject.SetActive(false);
        UIHighlight.Hide();

        // 인트로 / 역할 팝업 초기화
        if (IntroPanel != null)
        {
            IntroPanel.DOKill();
            IntroPanel.alpha = 0;
            IntroPanel.gameObject.SetActive(false);
        }
        if (RoleInfoPanel != null && RoleInfoPanel.IsOpen)
        {
            RoleInfoPanel.Close();
        }

        if (RoleGuessManager.Instance != null)
        {
            RoleGuessManager.Instance.ResetAllMarksToDefault();
        }

        // 3. 내부 상태 플래그 초기화
        _isShowingIntro = false;
        _isShowingRolePopup = false;
        _wasBlockerActive = false;

        // 4. 진행 중인 Yarn 대화 중단
        if (DialogueRunner != null && DialogueRunner.IsDialogueRunning)
        {
            DialogueRunner.Stop();
        }

        // 5. ExecutionManager 초기화 (탄환 복구, 조준 해제)
        if (ExecutionManager.Instance != null)
        {
            ExecutionManager.Instance.ResetState();
        }

        // 6. 시나리오 핸들러 초기화 (캐릭터 포함)
        _scenarioHandler.ResetState();
    }

    #region 스텝 진행

    private void ShowStep(int index)
    {
        var step = _currentStage.GetStep(index);
        if (step == null)
        {
            EndStage();
            return;
        }

        step.OnStepStart?.Invoke();
        
        // UI 설정
        DescText.text = step.Message;
        
        // 차단막(Blocker) 및 하이라이트 처리
        HandleStepType(step);
    }

    private void HandleStepType(TutorialStep step)
    {
        // 기본 시각 요소 설정
        bool hasMessage = !string.IsNullOrEmpty(step.Message);
        DialogueBox.gameObject.SetActive(hasMessage);

        if (NextButton != null) NextButton.gameObject.SetActive(false);
        
        if (Blocker != null) 
        {
            Blocker.gameObject.SetActive(true); // 기본적으로 모든 입력 차단
            if (_blockerFilter != null) _blockerFilter.Clear(); // 초기화
        }

        Vector2 targetPos = step.DialoguePosition;
        bool hasTarget = step.TargetObject != null;

        // 대화창 위치 설정
        if (hasMessage)
        {
            if (hasTarget && !step.UseCustomPos)
            {
                targetPos = CalculateDialoguePos(step.TargetObject.transform);
            }
            else if (!step.UseCustomPos)
            {
                targetPos = new Vector2(0,-450);
            }
            
            // 대화창 이동
            DialogueBox.DOAnchorPos(targetPos, MoveDuration).SetEase(Ease.OutCubic);
        }

        // 단계 타입별 로직 분기
        switch (step.Type)
        {
            case StepType.Dialogue:
                if (NextButton != null) NextButton.gameObject.SetActive(true);
                if (hasTarget && step.UseHighlight) UIHighlight.ApplyHighlight(step); 
                else UIHighlight.Hide();
                break;

            case StepType.ClickTarget:
                if (hasTarget)
                {
                    if (step.UseHighlight) UIHighlight.ApplyHighlight(step);
                    
                    if (_blockerFilter != null)
                    {
                        _blockerFilter.SetTarget(step.TargetObject, step.WorldCircleSize);
                    }

                    var trigger = step.TargetObject.GetComponent<TutorialTargetTrigger>();
                    if (trigger == null) step.TargetObject.AddComponent<TutorialTargetTrigger>();
                }
                break;

            case StepType.Wait:
                UIHighlight.Hide();
                if (Blocker != null) Blocker.gameObject.SetActive(false); 
                break;
                
            case StepType.ScenarioTestimony:
                _scenarioHandler.HandleTestimony(step);
                break;
                
            case StepType.ScenarioAnswer:
                _scenarioHandler.HandleAnswer(step);
                break;
        }
    }

    // 대화 모드에서 "다음" 버튼 클릭 시 호출
    private void OnNextButtonClicked()
    {
        if (_currentStage == null) return;
        var step = _currentStage.GetStep(_currentStepIndex);
        
        if (step != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.PageFlip);
            if (step.Type == StepType.Dialogue)
            {
                CompleteStep();
            }
            else if (step.Type == StepType.ScenarioTestimony && _scenarioHandler.IsShowingTestimony)
            {
                _scenarioHandler.CompleteTestimony();
            }
        }
    }

    // 외부 스크립트나 UI 트리거가 작업 완료 시 호출
    public void CompleteStep()
    {
        if (_currentStage == null) return;
        
        var step = _currentStage.GetStep(_currentStepIndex);
        if (step != null)
        {
            step.OnStepComplete?.Invoke();
            
            if (_blockerFilter != null) _blockerFilter.Clear();

            if (step.TargetObject != null)
            {
                var trigger = step.TargetObject.GetComponent<TutorialTargetTrigger>();
                if (trigger != null) Destroy(trigger);
            }
        }

        _currentStepIndex++;
        ShowStep(_currentStepIndex);
    }
    
    private void EndStage()
    {
        _currentStage.OnStageEnd?.Invoke();
        UIHighlight.Hide();
        DialogueBox.gameObject.SetActive(false);
        if (Blocker != null) Blocker.gameObject.SetActive(false);
        if (_blockerFilter != null) _blockerFilter.Clear();
    }

    #endregion

    #region 인트로 및 역할 설명 시퀀스

    /// <summary>
    /// 인트로 -> 역할 설명 -> 스텝 순서로 진행
    /// </summary>
    private IEnumerator PlayStageIntroSequence()
    {
        // 1. 스테이지 인트로 (페이드 인/아웃)
        if (_currentStage.IntroData.ShowIntro && IntroPanel != null)
        {
            _isShowingIntro = true;
            
            if (IntroRoundText != null) IntroRoundText.text = _currentStage.IntroData.RoundTitle;
            if (IntroSituationText != null) IntroSituationText.text = _currentStage.IntroData.SituationDescription;
            
            if (Blocker != null)
            {
                Blocker.gameObject.SetActive(true);
                if (_blockerFilter != null) _blockerFilter.Clear();
            }

            IntroPanel.gameObject.SetActive(true);
            IntroPanel.alpha = 1;
            
            yield return new WaitForSeconds(IntroDisplayDuration);
            
            yield return IntroPanel.DOFade(0f, IntroFadeDuration).WaitForCompletion();
            IntroPanel.gameObject.SetActive(false);
            
            _isShowingIntro = false;
        }

        // 2. 역할 설명 팝업
        if (_currentStage.RoleDatabase != null && 
            _currentStage.StageRoleNames != null &&
            _currentStage.StageRoleNames.Count > 0)
        {
            ShowRolePopup();
            yield return new WaitWhile(() => _isShowingRolePopup);
        }

        // 3. 튜토리얼 스텝 시작
        ShowStep(_currentStepIndex);
    }

    public void ShowAllRoles()
    {
        if (_isShowingRolePopup) return;

        if (RoleInfoPanel == null)
        {
            return;
        }

        _isShowingRolePopup = true;

        _wasBlockerActive = Blocker != null && Blocker.gameObject.activeSelf;
        if (Blocker != null)
        {
            Blocker.gameObject.SetActive(true);
        }

        RoleInfoPanel.OnPanelClosed -= OnRolePanelClosed;
        RoleInfoPanel.OnPanelClosed += OnRolePanelClosed;
        RoleInfoPanel.Open();
    }
    public void ShowRolePopup()
    {
        if (_isShowingRolePopup) return;

        if (RoleInfoPanel == null)
        {
            _isShowingRolePopup = false;
            return;
        }

        var currentRoleList = _currentStage.RoleDatabase.GetRoles(_currentStage.StageRoleNames);
        if (currentRoleList == null || currentRoleList.Count == 0)
        {
            _isShowingRolePopup = false;
            return;
        }

        _isShowingRolePopup = true;

        _wasBlockerActive = Blocker != null && Blocker.gameObject.activeSelf;
        if (Blocker != null)
        {
            Blocker.gameObject.SetActive(true);
        }

        RoleInfoPanel.OnPanelClosed -= OnRolePanelClosed;
        RoleInfoPanel.OnPanelClosed += OnRolePanelClosed;
        RoleInfoPanel.Open(currentRoleList);
    }

    private void OnRolePanelClosed()
    {
        if (RoleInfoPanel != null) RoleInfoPanel.OnPanelClosed -= OnRolePanelClosed;
        
        _isShowingRolePopup = false;

        if (Blocker != null)
        {
            Blocker.gameObject.SetActive(_wasBlockerActive);
        }
    }

    #endregion

    #region 보조 메서드 (Helper Methods)

    private Vector2 CalculateDialoguePos(Transform target)
    {
        Vector2 targetScreenPos = GetScreenPosition(target);
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
             DialogueBox.parent as RectTransform, 
             targetScreenPos, 
             null,
             out Vector2 localPos);

         float direction = (targetScreenPos.x < Screen.width / 2f) ? 1f : -1f;
         return new Vector2(localPos.x + (HorizontalOffset * direction), localPos.y + VerticalOffset);
    }

    private Vector2 GetScreenPosition(Transform target)
    {
        if (target.GetComponent<RectTransform>() != null)
        {
             return RectTransformUtility.WorldToScreenPoint(null, target.position);
        }
        return Camera.main.WorldToScreenPoint(target.position);
    }
    
    // 상태 확인용 프로퍼티
    public bool IsRunning => _currentStage != null;

    public TutorialStep GetCurrentStep()
    {
        if (_currentStage == null || _currentStepIndex < 0) return null;
        return _currentStage.GetStep(_currentStepIndex);
    }

    #endregion

    #region 시나리오 핸들러 위임 (외부 호출 인터페이스)

    // TutorialTargetTrigger 등 외부에서 호출하는 메서드들을 핸들러로 위임
    public void OnScenarioCharacterClicked(CharacterAI character) => _scenarioHandler.OnCharacterClicked(character);
    public void CompleteScenarioTestimony() => _scenarioHandler.CompleteTestimony();
    public void OnSkipButtonClicked()
    {
        _scenarioHandler.OnSkipButtonClicked();
    }
    public bool IsCorrectExecutionTarget(CharacterAI target) => _scenarioHandler.IsCorrectExecutionTarget(target);
    public void ShowWrongExecutionFeedback() => _scenarioHandler.ShowWrongExecutionFeedback();
    public void OnScenarioCharacterExecuted(CharacterAI character) => _scenarioHandler.OnCharacterExecuted(character);
    public List<RoleEntry> GetCurrentScenarioNewRoles() => _scenarioHandler.GetCurrentNewRoles();

    public IEnumerator ShowSkipScreen()
    {
        if (SkipBlackScreen == null) yield break;

        SkipBlackScreen.gameObject.SetActive(true);
        SkipBlackScreen.alpha = 0f;
        yield return SkipBlackScreen.DOFade(1f, 0.1f).WaitForCompletion();
        
        if (ExecutedCharacterNameText != null)
        {
            string characterName = GetLastExecutedCharacterName();
            ExecutedCharacterNameText.text = characterName + "이(가) 마녀에게 살해당했습니다.";
            ExecutedCharacterNameText.gameObject.SetActive(true);

            yield return new WaitForSeconds(ExecutedCharacterNameDisplayDuration);
            ExecutedCharacterNameText.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);
        yield return SkipBlackScreen.DOFade(0f, 0.3f).WaitForCompletion();
        SkipBlackScreen.gameObject.SetActive(false);
    }

    private string GetLastExecutedCharacterName()
    {
        // 마지막 처형된 캐릭터 이름을 가져옴 (TutorialCharacterAI의 DisplayName 참조)
        CharacterAI lastExecuted = _scenarioHandler.GetLastExecutedCharacter();
        return lastExecuted != null ? lastExecuted.DisplayName : "Unknown";
    }

    // 시나리오 모드 상태 확인용 프로퍼티 (외부 호환성)
    public bool IsScenarioMode => _scenarioHandler.IsScenarioMode;
    public bool IsWaitingForAnswer => _scenarioHandler.IsWaitingForAnswer;
    public bool IsShowingTestimony => _scenarioHandler.IsShowingTestimony;

    private void OnDialogueNodeComplete(string nodeName)
    {
        _scenarioHandler.OnDialogueNodeComplete();
    }

    #endregion
}
