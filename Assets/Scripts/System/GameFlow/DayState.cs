public sealed class DayState : GameFlowStateBase
{
    public override string Name => "Day";
    public override bool CanSkipDay => true;

    public DayState(IGameFlowContext context) : base(context)
    {
    }

    public override void Enter()
    {
        Context.SetTransitioning(false);
        Context.SetNight(false);
        Context.ResetTimer();
    }

    public override void OnTimerElapsed()
    {
        Context.ChangeState(new NightResolutionState(Context));
    }

    public override void OnCharacterExecuted(CharacterAI victim)
    {
        if (!Context.TryApplyExecution(victim)) return;

        if (Context.IsWinConditionMet())
        {
            Context.ChangeState(new LevelTransitionState(Context));
        }
    }
}
