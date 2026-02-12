using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 튜토리얼 타겟 클릭 트리거
/// 시나리오 모드에서 CharacterInteraction보다 먼저 클릭을 처리합니다.
/// </summary>
[DefaultExecutionOrder(-100)]  // CharacterInteraction보다 먼저 실행
public class TutorialTargetTrigger : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private bool _isTriggered = false;
    private CharacterInteraction _cachedInteraction;
    private CharacterVisual _cachedVisual;
    private bool _isHovering = false;

    private void Awake()
    {
        _cachedInteraction = GetComponent<CharacterInteraction>();
        _cachedVisual = GetComponent<CharacterVisual>();
    }

    // 마우스 호버 시작
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsScenarioMode()) return;
        
        _isHovering = true;
        
        // 이미 증언 중인 캐릭터가 있으면 호버 포커스 적용 안 함
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsShowingTestimony)
        {
            return;
        }
        
        // CharacterInteraction이 비활성화된 상태에서 호버 효과 적용
        if (_cachedVisual != null && (_cachedInteraction == null || !_cachedInteraction.enabled))
        {
            _cachedVisual.SetFocus(true);
        }
    }

    // 마우스 호버 종료
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsScenarioMode()) return;
        
        _isHovering = false;
        
        // 증언 중이 아닐 때만 포커스 해제
        if (_cachedVisual != null && (_cachedInteraction == null || !_cachedInteraction.enabled))
        {
            if (TutorialManager.Instance == null || !TutorialManager.Instance.IsShowingTestimony)
            {
                _cachedVisual.SetFocus(false);
            }
        }
    }

    // 클릭 감지 (Physics2DRaycaster 필요)
    public void OnPointerClick(PointerEventData eventData)
    {
        // 시나리오 모드에서 처리 시 기존 대화 차단
        if (HandleScenarioClick())
        {
            // [Fix] 처형 등으로 인해 오브젝트가 비활성화된 경우 코루틴 시작 불가하므로 리턴
            if (!gameObject.activeInHierarchy) return;

            // CharacterInteraction 일시적으로 비활성화하여 기존 대화 차단
            if (_cachedInteraction != null && _cachedInteraction.enabled)
            {
                StartCoroutine(DisableInteractionTemporarily());
            }
            return;
        }

        // 기존 튜토리얼 동작
        HandleNormalClick();
    }

    private IEnumerator DisableInteractionTemporarily()
    {
        _cachedInteraction.enabled = false;
        yield return null;  // 한 프레임 대기
        _cachedInteraction.enabled = true;
    }

    private bool IsScenarioMode()
    {
        if (TutorialManager.Instance == null) return false;
        
        var step = TutorialManager.Instance.GetCurrentStep();
        if (step == null) return false;
        
        return step.Type == StepType.ScenarioTestimony || step.Type == StepType.ScenarioAnswer;
    }

    private bool HandleScenarioClick()
    {
        if (TutorialManager.Instance == null) return false;
        
        var step = TutorialManager.Instance.GetCurrentStep();
        if (step == null) return false;

        // 시나리오 증언 모드 또는 정답 대기 모드
        if (step.Type == StepType.ScenarioTestimony || step.Type == StepType.ScenarioAnswer)
        {
            var character = GetComponent<CharacterAI>();
            if (character != null)
            {
                // [추가] 처형 모드(장전) 중이라면 처형 로직 우선 처리
                if (ExecutionManager.Instance != null && ExecutionManager.Instance.IsAiming)
                {
                    // 겁쟁이 캐릭터 특별 처리: 처형 전 대사 출력
                    if (character.MyRole == Role.Roles.겁쟁이)
                    {
                        // 하드코딩된 노드 이름 사용 (튜토리얼 03 캐릭터 1번 전용)
                        string nodeName = "Tutorial_Char1_03_Coward_Execution";
                        
                        if (TutorialManager.Instance.DialogueRunner != null && 
                            TutorialManager.Instance.DialogueRunner.Dialogue.NodeExists(nodeName))
                        {
                            // 처형 대상 예약 (Yarn 커맨드 execute_pending 용)
                            ExecutionManager.Instance.SetPendingTarget(character);
                            
                            // 조준 해제
                            ExecutionManager.Instance.ToggleAiming(false);

                            // 상호작용 시 Dialogue박스 (튜토리얼 메시지 박스) 비활성화
                            if (TutorialManager.Instance.DialogueBox != null)
                            {
                                TutorialManager.Instance.DialogueBox.gameObject.SetActive(false);
                            }

                            // 대화 시작
                            TutorialManager.Instance.DialogueRunner.StartDialogue(nodeName);

                            // 3. 상호작용 일시 비활성화 (대화 중 클릭 방지)
                            if (_cachedInteraction != null) _cachedInteraction.enabled = false;
                            
                            // 4. 대화 종료 후 복구 코루틴 시작
                            StartCoroutine(RestoreStateAfterDialogue());
                        }
                        return true;
                    }
                    
                    if (TutorialManager.Instance.IsCorrectExecutionTarget(character))
                    {
                        ExecutionManager.Instance.ExecuteTarget(character);
                        // ExecutionManager가 튜토리얼 매니저를 호출하지 않으므로 여기서 직접 호출
                        TutorialManager.Instance.OnScenarioCharacterExecuted(character);
                    }
                    else
                    {
                         TutorialManager.Instance.ShowWrongExecutionFeedback();
                         // CharacterInteraction이 IsAiming 상태를 확인하여 대사를 막을 수 있도록, 상태 변경을 프레임 대기 후 처리
                         StartCoroutine(DelayedToggleAimingOff());
                    }
                    return true; // 이벤트 처리됨 (CharacterInteraction 호출 안됨)
                }

                // 일반 클릭 (증언/힌트)
                Debug.Log($"[TutorialTargetTrigger] 시나리오 캐릭터 클릭: {gameObject.name}");
                TutorialManager.Instance.OnScenarioCharacterClicked(character);
                return true;  // 이벤트 처리됨
            }
        }
        
        return false;
    }

    private void HandleNormalClick()
    {
        if (_isTriggered) return;

        Debug.Log($"[TutorialTargetTrigger] Target Clicked: {gameObject.name}");

        if (TutorialManager.Instance == null) return;
        
        var step = TutorialManager.Instance.GetCurrentStep();
        if (step == null) return;

        // 기존 동작 (ClickTarget 등)
        _isTriggered = true;
        TutorialManager.Instance.CompleteStep();
    }

    /// <summary>
    /// 트리거 상태 리셋
    /// </summary>
    public void ResetTrigger()
    {
        _isTriggered = false;
        _isHovering = false;
    }
    private IEnumerator DelayedToggleAimingOff()
    {
        yield return null;
        if (ExecutionManager.Instance != null && ExecutionManager.Instance.IsAiming)
        {
            ExecutionManager.Instance.ToggleAiming(false);
        }
    }
    private IEnumerator RestoreStateAfterDialogue()
    {
        // 1. 대화 시작될 때까지 대기 (최대 1초)
        float timeout = 1.0f;
        while (timeout > 0)
        {
            if (TutorialManager.Instance?.DialogueRunner?.IsDialogueRunning == true) break;
            timeout -= Time.deltaTime;
            yield return null;
        }

        // 2. 대화 끝날 때까지 대기
        while (TutorialManager.Instance?.DialogueRunner?.IsDialogueRunning == true)
        {
            yield return null;
        }
        
        if (TutorialManager.Instance != null && TutorialManager.Instance.DialogueBox != null)
        {
             var step = TutorialManager.Instance.GetCurrentStep();
             bool shouldShow = (step != null && !string.IsNullOrEmpty(step.Message));
             TutorialManager.Instance.DialogueBox.gameObject.SetActive(shouldShow);
        }
    }
}
