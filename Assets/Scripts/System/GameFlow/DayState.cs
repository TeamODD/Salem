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
        Context.RecordRoundEndMetrics();
        Context.ChangeState(new NightResolutionState(Context));
    }

    public override void OnCharacterExecuted(CharacterAI victim)
    {
        if (!Context.TryApplyExecution(victim)) return;
        Context.RecordRoundEndMetrics();

        if (Context.IsWinConditionMet())
        {
            Context.ChangeState(new LevelTransitionState(Context));
            return;
        }

        // 처형으로 사망이 확정되면 남은 낮 시간을 건너뛰고 즉시 밤으로 전환한다.
        Context.ChangeState(new NightResolutionState(Context));
    }
}
