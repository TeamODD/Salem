using UnityEngine;

public class InsomniacAI : CharacterAI
{
    public override void DoNightAction(AIContext context)
    {
        if (context.IsEvenNight())
        {
            Debug.Log($"[Insomniac] {DisplayName} -> 산책 나감 (짝수 날)");
            SetAction(context, "insomniac_walk");
        }
        else
        {
            Debug.Log($"[Insomniac] {DisplayName} -> 집에 머무름 (홀수 날)");
            SetAction(context, "insomniac_home");
        }
    }

    public override void RecordDialogue(AIContext context)
    {
        if (lastAction == null)
        {
            AddDialogue("insomniac_home");
            return;
        }

        AddDialogue(lastAction.ActionId);
    }

    public override void ResolveMorning(AIContext context)
    {
        // TODO: 불면증 결과 로직 연결 지점
    }
}
