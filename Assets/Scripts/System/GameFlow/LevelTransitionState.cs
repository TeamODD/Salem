using System.Collections;

public sealed class LevelTransitionState : GameFlowStateBase
{
    public override string Name => "LevelTransition";

    public LevelTransitionState(IGameFlowContext context) : base(context)
    {
    }

    public override void Enter()
    {
        Context.SetTransitioning(true);
        Context.SetNight(false);
        Context.StopTimer();
        Context.StartManagedRoutine(TransitionRoutine());
    }

    public override void Exit()
    {
        Context.StopManagedRoutine();
    }

    private IEnumerator TransitionRoutine()
    {
        yield return Context.FadeOutRoutine(Context.FadeDuration);
        int nextLevel = Context.CurrentLevelIndex + 1;
        Context.ChangeState(new LevelLoadingState(Context, nextLevel));
    }
}
