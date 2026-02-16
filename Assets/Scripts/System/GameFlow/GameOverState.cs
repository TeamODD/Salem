using System.Collections;

public sealed class GameOverState : GameFlowStateBase
{
    public override string Name => "GameOver";

    public GameOverState(IGameFlowContext context) : base(context)
    {
    }

    public override void Enter()
    {
        Context.SetTransitioning(true);
        Context.SetNight(false);
        Context.StopTimer();
        Context.StartManagedRoutine(GameOverRoutine());
    }

    public override void Exit()
    {
        Context.StopManagedRoutine();
    }

    private IEnumerator GameOverRoutine()
    {
        yield return Context.FadeOutRoutine(Context.FadeDuration);
        Context.ShowDefeatUI();
    }
}
