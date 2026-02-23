using UnityEngine;

public class CowardAI : CharacterAI
{
    // 겁쟁이는 무조건 기도를 거부함
    public override bool WillRefusePrayer => true;

    public override void DoNightAction(AIContext context)
    {
        Debug.Log($"[Coward] {DisplayName} -> 집에 머무름 (겁쟁이)");
        SetAction(context, AIActionType.CowardPlea);
    }

    public override void ResolveMorning(AIContext context)
    {
        // 겁쟁이 결과 로직
    }
}
