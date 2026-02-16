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
    }
}
