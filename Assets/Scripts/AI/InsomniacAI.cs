public class InsomniacAI : CharacterAI
{
    public override void DoNightAction(AIContext context)
    {
        if (context.IsEvenNight())
        {
            SetAction(context, "insomniac_walk");
        }
        else
        {
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
