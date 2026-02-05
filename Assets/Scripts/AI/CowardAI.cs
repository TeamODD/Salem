using UnityEngine;

public class CowardAI : CharacterAI
{
    public override void DoNightAction(AIContext context)
    {
        Debug.Log($"[Coward] {DisplayName} -> 집에 머무름 (겁쟁이)");
        SetAction(context, "coward_plea");
    }

    public override void ResolveMorning(AIContext context)
    {
        // 겁쟁이 결과 로직
    }
}