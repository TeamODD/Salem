using UnityEngine;

public class MuteAI : CharacterAI
{
    public override void DoNightAction(AIContext context)
    {
        Debug.Log($"[Mute] {DisplayName} -> 집에 머무름 (벙어리)");
        SetAction(context, "mute_silent");
    }

    public override void ResolveMorning(AIContext context)
    {
        // 벙어리 결과 로직
    }
}