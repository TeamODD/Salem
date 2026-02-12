using UnityEngine;
using Yarn.Unity; // Yarn Spinner 패키지가 있어야 함

public class TutorialYarnBridge : MonoBehaviour
{
    [Tooltip("체크하면 대화(Dialogue)가 종료될 때 자동으로 튜토리얼 단계를 완료합니다.")]
    public bool AdvanceOnDialogueComplete = true;

    private DialogueRunner _dialogueRunner;

    void Awake()
    {
        _dialogueRunner = GetComponent<DialogueRunner>();
        if (_dialogueRunner != null)
        {
            _dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        }
    }

    private void OnDialogueComplete()
    {
        if (!AdvanceOnDialogueComplete) return;

        // 튜토리얼 매니저가 존재하고, 현재 실행 중일 때만 동작
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsRunning)
        {
            var step = TutorialManager.Instance.GetCurrentStep();
            
            if (step == null) return;
            
            // 시나리오 증언 타입일 경우 증언 완료 처리
            if (step.Type == StepType.ScenarioTestimony)
            {
                TutorialManager.Instance.CompleteScenarioTestimony();
            }
            // 현재 단계가 'Wait' 타입일 때만 대화 종료 시 자동으로 넘어가도록 제한
            else if (step.Type == StepType.Wait)
            {
                TutorialManager.Instance.CompleteStep();
            }
        }
    }

    // Yarn 스크립트에서 직접 호출할 수 있는 커맨드
    // 사용법: <<tutorial_next>>
    [YarnCommand("tutorial_next")]
    public static void TutorialNext()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsRunning)
        {
            TutorialManager.Instance.CompleteStep();
        }
    }
}
