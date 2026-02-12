using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 튜토리얼 시나리오 대본 데이터 (ScriptableObject)
/// 각 라운드의 캐릭터 배치, 대사, 정답을 정의합니다.
/// </summary>
[CreateAssetMenu(fileName = "TutorialScenario", menuName = "Tutorial/Scenario Data")]
public class TutorialScenarioData : ScriptableObject
{
    [Header("시나리오 기본 정보")]
    public string ScenarioName = "새 시나리오";
    
    [TextArea(2, 4)]
    public string ScenarioDescription = "";

    [Header("라운드 목록")]
    public List<ScenarioRound> Rounds = new List<ScenarioRound>();

    /// <summary>
    /// 특정 라운드 데이터를 가져옴
    /// </summary>
    public ScenarioRound GetRound(int roundIndex)
    {
        if (roundIndex >= 0 && roundIndex < Rounds.Count)
            return Rounds[roundIndex];
        return null;
    }
}

/// <summary>
/// 한 라운드(밤+낮)의 시나리오 데이터
/// </summary>
[Serializable]
public class ScenarioRound
{
    [Header("라운드 정보")]
    public string RoundTitle = "1라운드";
    
    [TextArea(2, 4)]
    public string RoundDescription = "";

    [Header("이 라운드에서 새로 소개할 역할")]
    public List<string> NewRoleNames = new List<string>();

    [Header("캐릭터 역할 배치 (인덱스 = 캐릭터 번호)")]
    [Tooltip("캐릭터 0, 1, 2, 3... 순서대로 역할 지정")]
    public List<CharacterRoleAssignment> CharacterRoles = new List<CharacterRoleAssignment>();

    [Header("낮 단계 - 대화 순서")]
    [Tooltip("각 DayPhase는 낮 동안의 대화 진행을 정의합니다 (복수의 캐릭터 증언 포함)")]
    public List<DayPhase> DayPhases = new List<DayPhase>();

    [Header("정답 액션")]
    public AnswerAction CorrectAnswer = new AnswerAction();

    [Header("라운드 종료 후 사망 처리")]
    [Tooltip("이 라운드가 끝난 후 사망할 캐릭터 인덱스 (-1이면 없음)")]
    public int DeathCharacterIndex = -1;
}

/// <summary>
/// 캐릭터 역할 배치 정보
/// </summary>
[Serializable]
public class CharacterRoleAssignment
{
    public int CharacterIndex;
    public Role.Roles AssignedRole;
    
    [Tooltip("라운드 시작 시 이미 사망 상태인지")]
    public bool IsDeadAtStart = false;
}

/// <summary>
/// 낮 단계(대화 페이즈) - 여러 캐릭터의 증언을 순서대로 정의
/// </summary>
[Serializable]
public class DayPhase
{
    [Header("페이즈 정보")]
    public string PhaseName = "증언 단계";
    
    [Header("캐릭터 증언 목록")]
    [Tooltip("이 페이즈에서 증언할 캐릭터들의 대사")]
    public List<CharacterTestimony> Testimonies = new List<CharacterTestimony>();
}

/// <summary>
/// 개별 캐릭터의 증언 데이터
/// </summary>
[Serializable]
public class CharacterTestimony
{
    [Header("증언 캐릭터")]
    [Tooltip("증언하는 캐릭터의 인덱스 (0부터 시작)")]
    public int CharacterIndex;

    [Header("대사 설정")]
    [Tooltip("Yarn 노드 이름 (비어있으면 DirectDialogue 사용)")]
    public string YarnNodeName = "";
    
    [TextArea(3, 10)]
    [Tooltip("Yarn을 사용하지 않을 경우 직접 입력할 대사")]
    public string DirectDialogue = "";

    [Header("추가 정보 (툴팁/힌트)")]
    [TextArea(2, 4)]
    [Tooltip("플레이어에게 보여줄 힌트 또는 추론 포인트")]
    public string HintText = "";
}

/// <summary>
/// 정답 액션 정의
/// </summary>
[Serializable]
public class AnswerAction
{
    [Header("정답 타입")]
    public AnswerType Type = AnswerType.Skip;
    
    [Header("처형 대상 (Type이 Execute일 경우)")]
    [Tooltip("처형해야 할 캐릭터 인덱스")]
    public int TargetCharacterIndex = -1;

    [Header("오답 시 피드백")]
    [TextArea(2, 4)]
    public string WrongAnswerMessage = "다시 생각해보세요.";
    
    [Header("정답 시 피드백")]
    [TextArea(2, 4)]
    public string CorrectAnswerMessage = "정확합니다!";
}

/// <summary>
/// 정답 타입 열거형
/// </summary>
public enum AnswerType
{
    Skip,       // 넘기기 (처형 안 함)
    Execute     // 특정 캐릭터 처형
}
