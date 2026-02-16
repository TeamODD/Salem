public sealed class LevelLoadingState : GameFlowStateBase
{
    private readonly int levelIndex;

    public override string Name => $"LevelLoading({levelIndex})";

    public LevelLoadingState(IGameFlowContext context, int levelIndex) : base(context)
    {
        this.levelIndex = levelIndex;
    }

    public override void Enter()
    {
        Context.SetTransitioning(true);
        Context.SetNight(false);
        Context.StopTimer();
        Context.SetFadeOpaque();
        Context.StartManagedRoutine(Context.LoadLevelRoutine(levelIndex));
    }

    public override void Exit()
    {
        Context.StopManagedRoutine();
    }
}
