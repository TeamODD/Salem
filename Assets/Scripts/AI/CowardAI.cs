using UnityEngine;

public class CowardAI : CharacterAI
{
    public override void DoNightAction(AIContext context)
    {
        Debug.Log($"[Coward] {DisplayName} -> 집에 머무름 (겁쟁이)");
        SetAction(context, "coward_plea");
    }

    public override void RecordDialogue(AIContext context)
    {
        AddDialogue("coward_plea");
    }

    public override void ResolveMorning(AIContext context)
    {
        // TODO: 겁쟁이 결과 로직 연결 지점
    }
}
