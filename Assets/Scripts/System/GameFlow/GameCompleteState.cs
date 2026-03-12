using System.Collections;

public sealed class GameCompleteState : GameFlowStateBase
{
    public override string Name => "GameComplete";

    public GameCompleteState(IGameFlowContext context) : base(context)
    {
    }

    public override void Enter()
    {
        Context.SetTransitioning(true);
        Context.SetNight(false);
        Context.StopTimer();
        Context.StartManagedRoutine(GameCompleteRoutine());
    }

    public override void Exit()
    {
        Context.StopManagedRoutine();
    }

    private IEnumerator GameCompleteRoutine()
    {
        yield return Context.FadeOutRoutine(Context.FadeDuration);
        Context.FinalizeScoreAndOpenResult(true);
    }
}
