using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using Yarn.Unity;

/// <summary>
/// 튜토리얼 시나리오 모드 전담 핸들러
/// TutorialManager에서 시나리오 관련 로직을 위임받아 처리합니다.
/// </summary>
public class TutorialScenarioHandler : MonoBehaviour
{
    // TutorialManager에서 주입받는 참조
    private List<CharacterAI> _characters;
    private DialogueRunner _dialogueRunner;
    private RectTransform _dialogueBox;
    private TextMeshProUGUI _descText;
    private Button _nextButton;
    private UIHighlightController _uiHighlight;
    private Image _blocker;
    private TutorialRaycastFilter _blockerFilter;

    // 시나리오 모드 상태
    private TutorialStage _currentStage;
    private ScenarioRound _currentScenarioRound;
    private int _currentRoundIndex = 0;
    private int _currentTestimonyIndex = 0;
    private int _currentPhaseIndex = 0;
    private bool _isScenarioMode = false;
    private bool _isWaitingForAnswer = false;

    // 클릭 기반 증언 시스템
    private HashSet<int> _completedTestimonies = new HashSet<int>();
    private bool _isWaitingForCharacterClick = false;
    private bool _isShowingTestimony = false;

    private TutorialCharacterAI _lastExecutedCharacter;

    // 상태 확인용 프로퍼티
    public bool IsScenarioMode => _isScenarioMode;
    public bool IsWaitingForAnswer => _isWaitingForAnswer;
    public bool IsShowingTestimony => _isShowingTestimony;

    /// <summary>
    /// TutorialManager에서 필요한 참조를 주입받아 초기화
    /// </summary>
    public void Setup(
        List<CharacterAI> characters,
        DialogueRunner dialogueRunner,
        RectTransform dialogueBox,
        TextMeshProUGUI descText,
        Button nextButton,
        UIHighlightController uiHighlight,
        Image blocker,
        TutorialRaycastFilter blockerFilter)
    {
        _characters = characters;
        _dialogueRunner = dialogueRunner;
        _dialogueBox = dialogueBox;
        _descText = descText;
        _nextButton = nextButton;
        _uiHighlight = uiHighlight;
        _blocker = blocker;
        _blockerFilter = blockerFilter;
    }

    #region 초기화

    /// <summary>
    /// 시나리오 모드 초기화 (스테이지 시작 시 호출)
    /// </summary>
    public void Initialize(TutorialStage stage)
    {
        _currentStage = stage;

        if (_currentStage.ScenarioData == null)
        {
            _isScenarioMode = false;
            return;
        }

        _currentRoundIndex = _currentStage.StartRoundIndex;

        if (!InitializeRound(_currentRoundIndex))
        {
            _isScenarioMode = false;
            return;
        }

        _isScenarioMode = true;
    }

    /// <summary>
    /// 특정 라운드 초기화
    /// </summary>
    private bool InitializeRound(int roundIndex)
    {
        var round = _currentStage.ScenarioData.GetRound(roundIndex);
        if (round == null) return false;

        _currentScenarioRound = round;
        _currentPhaseIndex = 0;
        _currentTestimonyIndex = 0;
        _isWaitingForAnswer = false;
        _completedTestimonies.Clear();

        ApplyScenarioCharacterRoles();
        ApplyScenarioDeathStates();
        SetCharacterInteractionsEnabled(false);
        return true;
    }

    /// <summary>
    /// 모든 시나리오 상태 초기화 (스테이지 전환 시)
    /// </summary>
    public void ResetState()
    {
        _isScenarioMode = false;
        _isWaitingForAnswer = false;
        _isWaitingForCharacterClick = false;
        _isShowingTestimony = false;
        _currentScenarioRound = null;
        _currentRoundIndex = 0;
        _currentTestimonyIndex = 0;
        _currentPhaseIndex = 0;
        _completedTestimonies.Clear();
        _currentStage = null;
        _lastExecutedCharacter = null; // 처형된 캐릭터 초기화

        ResetScenarioCharacters();
    }

    /// <summary>
    /// 마지막으로 처형된 캐릭터 반환
    /// </summary>
    public CharacterAI GetLastExecutedCharacter()
    {
        return _lastExecutedCharacter;
    }

    #endregion

    #region 캐릭터 관리

    private void ApplyScenarioCharacterRoles()
    {
        if (_currentScenarioRound == null) return;

        foreach (var assignment in _currentScenarioRound.CharacterRoles)
        {
            if (assignment.CharacterIndex >= 0 && assignment.CharacterIndex < _characters.Count)
            {
                var character = _characters[assignment.CharacterIndex];
                if (character != null)
                {
                    character.SetRole(assignment.AssignedRole);
                }
            }
        }
    }

    private void ApplyScenarioDeathStates()
    {
        if (_currentScenarioRound == null) return;

        foreach (var assignment in _currentScenarioRound.CharacterRoles)
        {
            if (assignment.CharacterIndex >= 0 && assignment.CharacterIndex < _characters.Count)
            {
                var character = _characters[assignment.CharacterIndex];
                if (character != null)
                {
                    character.gameObject.SetActive(!assignment.IsDeadAtStart);
                }
            }
        }
    }

    private void SetCharacterInteractionsEnabled(bool enabled)
    {
        foreach (var character in _characters)
        {
            if (character == null) continue;

            var interaction = character.GetComponent<CharacterInteraction>();
            if (interaction != null)
            {
                interaction.enabled = enabled;
            }
        }
    }

    /// <summary>
    /// 스테이지 시작 시 캐릭터 상태 초기화
    /// </summary>
    public void ResetScenarioCharacters()
    {
        if (_characters == null) return;

        SetCharacterInteractionsEnabled(true);

        foreach (var character in _characters)
        {
            if (character == null) continue;

            character.gameObject.SetActive(true);

            var visual = character.GetComponent<CharacterVisual>();
            if (visual != null)
            {
                visual.SetFocus(false);
            }

            var trigger = character.GetComponent<TutorialTargetTrigger>();
            if (trigger != null)
            {
                DestroyImmediate(trigger);
            }
        }
    }

    #endregion

    #region 증언 처리

    /// <summary>
    /// 시나리오 증언 단계 처리 - 클릭 기반
    /// </summary>
    public void HandleTestimony(TutorialStep step)
    {
        if (!_isScenarioMode || _currentScenarioRound == null)
        {
            Debug.LogWarning("[TutorialScenarioHandler] 시나리오 모드가 활성화되지 않았습니다.");
            TutorialManager.Instance.CompleteStep();
            return;
        }

        if (_currentPhaseIndex >= _currentScenarioRound.DayPhases.Count)
        {
            _completedTestimonies.Clear();
            TutorialManager.Instance.CompleteStep();
            return;
        }

        var phase = _currentScenarioRound.DayPhases[_currentPhaseIndex];

        if (_completedTestimonies.Count >= phase.Testimonies.Count)
        {
            _currentPhaseIndex++;
            _completedTestimonies.Clear();
            HandleTestimony(step);
            return;
        }

        _isWaitingForCharacterClick = true;
        _isShowingTestimony = false;

        _dialogueBox.gameObject.SetActive(false);
        if (!string.IsNullOrEmpty(step.Message))
        {
            _dialogueBox.gameObject.SetActive(true);
            _descText.text = step.Message;
        }
        if (_nextButton != null) _nextButton.gameObject.SetActive(false);

        _uiHighlight.Hide();

        if (_blocker != null) _blocker.gameObject.SetActive(false);

        SetupCharacterClickTriggers(phase);
    }

    private void SetupCharacterClickTriggers(DayPhase phase)
    {
        foreach (var testimony in phase.Testimonies)
        {
            if (testimony.CharacterIndex < 0 || testimony.CharacterIndex >= _characters.Count)
                continue;

            var character = _characters[testimony.CharacterIndex];
            if (character == null || !character.gameObject.activeInHierarchy)
                continue;

            if (_completedTestimonies.Contains(testimony.CharacterIndex))
                continue;

            var trigger = character.GetComponent<TutorialTargetTrigger>();
            if (trigger == null)
            {
                trigger = character.gameObject.AddComponent<TutorialTargetTrigger>();
            }
        }
    }

    /// <summary>
    /// 캐릭터 클릭 시 호출 (TutorialTargetTrigger에서 호출)
    /// </summary>
    public void OnCharacterClicked(CharacterAI clickedCharacter)
    {
        if (_isShowingTestimony) return;

        if (_dialogueRunner != null && _dialogueRunner.IsDialogueRunning) return;

        if (!_isWaitingForCharacterClick && !_isWaitingForAnswer) return;

        int characterIndex = _characters.IndexOf(clickedCharacter);
        if (characterIndex < 0) return;

        int phaseIndex = _currentPhaseIndex;
        if (phaseIndex >= _currentScenarioRound.DayPhases.Count)
        {
            phaseIndex = _currentScenarioRound.DayPhases.Count - 1;
        }
        if (phaseIndex < 0) return;

        var phase = _currentScenarioRound.DayPhases[phaseIndex];
        var testimony = phase.Testimonies.Find(t => t.CharacterIndex == characterIndex);

        if (testimony == null) return;

        _isShowingTestimony = true;
        _isWaitingForCharacterClick = false;

        _dialogueBox.gameObject.SetActive(false);

        var visual = clickedCharacter.GetComponent<CharacterVisual>();
        if (visual != null)
        {
            visual.SetFocus(true);
        }

        _currentTestimonyIndex = characterIndex;

        if (!string.IsNullOrEmpty(testimony.YarnNodeName) && _dialogueRunner != null)
        {
            _dialogueRunner.StartDialogue(testimony.YarnNodeName);
        }
        else if (!string.IsNullOrEmpty(testimony.DirectDialogue))
        {
            _descText.text = testimony.DirectDialogue;
            _dialogueBox.gameObject.SetActive(true);
            if (_nextButton != null) _nextButton.gameObject.SetActive(true);
        }
        else if (_isWaitingForAnswer && !string.IsNullOrEmpty(testimony.HintText))
        {
            bool isCorrectTarget = false;
            if (_currentScenarioRound != null && _currentScenarioRound.CorrectAnswer != null)
            {
                var answer = _currentScenarioRound.CorrectAnswer;
                if (answer.Type == AnswerType.Execute && answer.TargetCharacterIndex == characterIndex)
                {
                    isCorrectTarget = true;
                }
            }

            if (!isCorrectTarget)
            {
                _descText.text = testimony.HintText;
                _dialogueBox.gameObject.SetActive(true);
                if (_nextButton != null) _nextButton.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 시나리오 증언 완료 (Yarn 대화 종료 또는 다음 버튼 클릭 시)
    /// </summary>
    public void CompleteTestimony()
    {
        if (!_isScenarioMode || !_isShowingTestimony) return;

        _completedTestimonies.Add(_currentTestimonyIndex);
        _isShowingTestimony = false;

        if (_currentTestimonyIndex >= 0 && _currentTestimonyIndex < _characters.Count)
        {
            var character = _characters[_currentTestimonyIndex];
            if (character != null)
            {
                var visual = character.GetComponent<CharacterVisual>();
                if (visual != null)
                {
                    visual.SetFocus(false);
                }
            }
        }

        var step = TutorialManager.Instance.GetCurrentStep();

        if (_isWaitingForAnswer && step != null && step.Type == StepType.ScenarioAnswer)
        {
            HandleAnswer(step);
            return;
        }

        if (step != null && step.Type == StepType.ScenarioTestimony)
        {
            HandleTestimony(step);
        }
    }

    #endregion

    #region 정답 및 처형 처리

    /// <summary>
    /// 시나리오 정답 대기 단계 처리
    /// </summary>
    public void HandleAnswer(TutorialStep step)
    {
        if (!_isScenarioMode || _currentScenarioRound == null)
        {
            Debug.LogWarning("[TutorialScenarioHandler] 시나리오 모드가 활성화되지 않았습니다.");
            TutorialManager.Instance.CompleteStep();
            return;
        }

        _isWaitingForAnswer = true;
        _isShowingTestimony = false;

        bool hasMessage = !string.IsNullOrEmpty(step.Message);
        _dialogueBox.gameObject.SetActive(hasMessage);
        if (hasMessage) _descText.text = step.Message;

        if (step.UseHighlight && step.TargetObject != null)
        {
            _uiHighlight.ApplyHighlight(step);
        }
        else
        {
            _uiHighlight.Hide();
        }

        if (_blocker != null)
        {
            _blocker.gameObject.SetActive(false);
            if (_blockerFilter != null)
            {
                _blockerFilter.Clear();
            }
        }
    }

    /// <summary>
    /// 넘기기 버튼 클릭 시 호출 (외부에서 연결)
    /// </summary>
    public void OnSkipButtonClicked()
    {
        SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
        if (!_isWaitingForAnswer) return;

        var answer = _currentScenarioRound?.CorrectAnswer;
        if (answer == null) return;

        if (answer.Type == AnswerType.Skip)
        {
            ShowFeedback(answer.CorrectAnswerMessage, true);
            _isWaitingForAnswer = false;
            // 스킵 화면 표시
            TutorialManager.Instance.StartCoroutine(TutorialManager.Instance.ShowSkipScreen());
            ProcessRoundEnd();
        }
        else
        {
            ShowFeedback(answer.WrongAnswerMessage, false);
        }
    }

    /// <summary>
    /// 처형 대상이 올바른지 확인
    /// </summary>
    public bool IsCorrectExecutionTarget(CharacterAI target)
    {
        if (!_isWaitingForAnswer || _currentScenarioRound == null) return false;

        var answer = _currentScenarioRound.CorrectAnswer;
        if (answer == null || answer.Type != AnswerType.Execute) return false;

        int characterIndex = _characters.IndexOf(target);
        return answer.TargetCharacterIndex == characterIndex;
    }

    /// <summary>
    /// 잘못된 처형 시도 시 피드백
    /// </summary>
    public void ShowWrongExecutionFeedback()
    {
        if (_currentScenarioRound?.CorrectAnswer != null)
        {
            ShowFeedback(_currentScenarioRound.CorrectAnswer.WrongAnswerMessage, false);
        }
    }

    /// <summary>
    /// 캐릭터 처형 시 호출
    /// </summary>
    public void OnCharacterExecuted(CharacterAI executedCharacter)
    {
        _lastExecutedCharacter = executedCharacter as TutorialCharacterAI; // 최근 처형 캐릭터 기록
        if (!_isWaitingForAnswer) return;

        int characterIndex = _characters.IndexOf(executedCharacter);
        if (characterIndex < 0) return;

        var answer = _currentScenarioRound?.CorrectAnswer;
        if (answer == null) return;

        if (answer.Type == AnswerType.Execute && answer.TargetCharacterIndex == characterIndex)
        {
            // 처형 대상 캐릭터 비활성화 (튜토리얼에서는 GameManager 상태 머신이
            // DayState가 아닐 수 있으므로 직접 처리)
            executedCharacter.gameObject.SetActive(false);

            ShowFeedback(answer.CorrectAnswerMessage, true);
            _isWaitingForAnswer = false;
            ProcessRoundEnd();
        }
        else
        {
            ShowFeedback(answer.WrongAnswerMessage, false);
        }
    }

    private void ShowFeedback(string message, bool isCorrect)
    {
        if (string.IsNullOrEmpty(message)) return;

        _descText.text = message;
        // TODO: 정답/오답에 따른 색상 변경 또는 애니메이션
    }

    #endregion

    #region 라운드 진행

    private void ProcessRoundEnd()
    {
        // 사망 처리
        if (_currentScenarioRound != null &&
            _currentScenarioRound.DeathCharacterIndex >= 0 &&
            _currentScenarioRound.DeathCharacterIndex < _characters.Count)
        {
            var deadCharacter = _characters[_currentScenarioRound.DeathCharacterIndex];
            if (deadCharacter != null)
            {
                _lastExecutedCharacter = deadCharacter as TutorialCharacterAI;
                deadCharacter.gameObject.SetActive(false);
            }
        }

        int nextRoundIndex = _currentRoundIndex + 1;
        bool hasNextRound = _currentStage.IsRoundInRange(nextRoundIndex);

        if (hasNextRound)
        {
            _currentRoundIndex = nextRoundIndex;

            if (InitializeRound(_currentRoundIndex))
            {
                TutorialManager.Instance.CompleteStep();
            }
            else
            {
                EndScenarioMode();
                TutorialManager.Instance.CompleteStep();
            }
        }
        else
        {
            EndScenarioMode();
            TutorialManager.Instance.CompleteStep();
        }
    }

    private void EndScenarioMode()
    {
        _currentScenarioRound = null;
        _isScenarioMode = false;

        SetCharacterInteractionsEnabled(true);
    }

    /// <summary>
    /// 현재 시나리오 라운드의 새 역할 목록 가져오기
    /// </summary>
    public List<RoleEntry> GetCurrentNewRoles()
    {
        if (_currentScenarioRound == null || _currentStage?.RoleDatabase == null)
            return new List<RoleEntry>();

        return _currentStage.RoleDatabase.GetRoles(_currentScenarioRound.NewRoleNames);
    }

    /// <summary>
    /// Yarn 대화 노드 완료 시 호출
    /// </summary>
    public void OnDialogueNodeComplete()
    {
        if (_isShowingTestimony)
        {
            CompleteTestimony();
        }
    }

    #endregion
}
