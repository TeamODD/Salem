public class CowardAI : CharacterAI
{
    public override void DoNightAction(AIContext context)
    {
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
