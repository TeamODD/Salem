using UnityEngine;

/// <summary>
/// 튜토리얼 전용 캐릭터 AI
/// 모든 역할을 처리할 수 있으며, 런타임에 역할 변경이 가능합니다.
/// 실제 AI 로직 없이 시나리오 데이터에 따라 동작합니다.
/// </summary>
public class TutorialCharacterAI : CharacterAI
{
    [Header("튜토리얼 설정")]
    [SerializeField] private int characterIndex = 0;  // TutorialManager의 ScenarioCharacters 인덱스
    
    public int CharacterIndex => characterIndex;

    /// <summary>
    /// 밤 행동 - 튜토리얼에서는 시나리오 데이터에 따라 자동 처리
    /// </summary>
    public override void DoNightAction(AIContext context)
    {
        // 튜토리얼에서는 밤 행동이 시나리오로 고정됨
        Debug.Log($"[TutorialCharacterAI] {DisplayName} 밤 행동 (시나리오 기반)");
    }

    /// <summary>
    /// 아침 해결 - 튜토리얼에서는 사용하지 않음
    /// </summary>
    public override void ResolveMorning(AIContext context)
    {
        // 튜토리얼에서는 아침 해결이 시나리오로 고정됨
        Debug.Log($"[TutorialCharacterAI] {DisplayName} 아침 해결 (시나리오 기반)");
    }

    /// <summary>
    /// 캐릭터 인덱스 설정
    /// </summary>
    public void SetCharacterIndex(int index)
    {
        characterIndex = index;
    }
}
