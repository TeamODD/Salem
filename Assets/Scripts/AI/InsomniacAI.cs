using UnityEngine;

public class InsomniacAI : CharacterAI
{
    public override void DoNightAction(AIContext context)
    {
        if (context.IsEvenNight())
        {
            Debug.Log($"[Insomniac] {DisplayName} -> 산책 나감 (짝수 날)");
            SetAction(context, AIActionType.InsomniacWalk);
        }
        else
        {
            Debug.Log($"[Insomniac] {DisplayName} -> 집에 머무름 (홀수 날)");
            SetAction(context, AIActionType.InsomniacHome);
        }
    }

    public override void ResolveMorning(AIContext context)
    {
        // 불면증 결과 로직
    }
}
