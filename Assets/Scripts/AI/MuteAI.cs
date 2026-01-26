public class MuteAI : CharacterAI
{
    public override void DoNightAction(AIContext context)
    {
        SetAction(context, "mute_silent");
    }

    public override void RecordDialogue(AIContext context)
    {
        AddDialogue("mute_silent");
    }

    public override void ResolveMorning(AIContext context)
    {
        // TODO: 벙어리 결과 로직 연결 지점
    }
}
