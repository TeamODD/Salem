public sealed class BootState : GameFlowStateBase
{
    public override string Name => "Boot";

    public BootState(IGameFlowContext context) : base(context)
    {
    }

    public override void Enter()
    {
        Context.ChangeState(new LevelLoadingState(Context, 0));
    }
}
