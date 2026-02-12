using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

[Serializable]
public class TutorialStep
{
    public string StepName = "새로운 단계"; // New Step
    public StepType Type = StepType.Dialogue;

    [TextArea(3, 10)]
    public string Message;
    
    [Header("타겟 설정")]
    public GameObject TargetObject; // UI 또는 월드 오브젝트
    public bool UseHighlight = true;
    
    [Header("대화 설정")]
    public Vector2 DialoguePosition;
    public bool UseCustomPos = false; // false면 자동 계산

    [Header("이벤트")]
    public UnityEvent OnStepStart;
    public UnityEvent OnStepComplete; // 단계 완료 조건 충족 시 호출

    [Header("월드 오브젝트의 경우 (500 = 기본 크기)")]
    public float WorldCircleSize = 500f;
    
    [Header("UI 오브젝트의 경우 (1.2 = 20% 패딩)")]
    public float UIPadding = 1.2f;
}

public enum StepType
{
    Dialogue,           // 텍스트 읽기 전용, 아무 곳이나 클릭하면 넘어감
    ClickTarget,        // 특정 타겟을 클릭해야 함
    Wait,               // 외부 이벤트나 타이머 대기
    ScenarioTestimony,  // 시나리오 모드: 캐릭터 증언 (Yarn 대화 + 클릭)
    ScenarioAnswer      // 시나리오 모드: 정답 입력 대기 (넘기기 or 처형)
}

/// <summary>
/// 스테이지 시작 시 표시되는 인트로 정보 (라운드 번호, 상황 설명)
/// </summary>
[Serializable]
public class StageIntroData
{
    public bool ShowIntro = false;
    public string RoundTitle = "1라운드";
    [TextArea(2, 5)]
    public string SituationDescription = "";
}

public class TutorialStage : MonoBehaviour
{
    public int StageIndex;
    [TextArea] public string StageDescription;

    [Header("인트로 설정 (페이드 인/아웃)")]
    public StageIntroData IntroData = new StageIntroData();

    [Header("역할 설명 설정")]
    [Tooltip("모든 역할 데이터가 저장된 ScriptableObject")]
    public RoleIntroData RoleDatabase;
    
    [Tooltip("이 스테이지에서 설명할 역할 이름 목록")]
    public List<string> StageRoleNames = new List<string>();
    
    public List<TutorialStep> Steps = new List<TutorialStep>();

    [Header("시나리오 모드 설정")]
    [Tooltip("시나리오 모드 사용 시 연결할 시나리오 데이터")]
    public TutorialScenarioData ScenarioData;
    
    [Tooltip("시작 라운드 인덱스 (0부터 시작)")]
    public int StartRoundIndex = 0;
    
    [Tooltip("종료 라운드 인덱스 (포함). -1이면 시작 라운드만 실행")]
    public int EndRoundIndex = -1;
    
    // 하위 호환성을 위한 프로퍼티
    public int ScenarioRoundIndex => StartRoundIndex;

    [Header("이벤트")]
    public UnityEvent OnStageStart;
    public UnityEvent OnStageEnd;

    public TutorialStep GetStep(int index)
    {
        if (index >= 0 && index < Steps.Count) return Steps[index];
        return null;
    }
    
    /// <summary>
    /// 이 스테이지에서 실행할 총 라운드 수
    /// </summary>
    public int TotalRounds
    {
        get
        {
            if (EndRoundIndex < 0) return 1;
            return EndRoundIndex - StartRoundIndex + 1;
        }
    }
    
    /// <summary>
    /// 주어진 라운드 인덱스가 이 스테이지 범위 내인지 확인
    /// </summary>
    public bool IsRoundInRange(int roundIndex)
    {
        if (EndRoundIndex < 0) return roundIndex == StartRoundIndex;
        return roundIndex >= StartRoundIndex && roundIndex <= EndRoundIndex;
    }
}
