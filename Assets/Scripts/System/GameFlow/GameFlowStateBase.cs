public abstract class GameFlowStateBase : IGameFlowState
{
    protected readonly IGameFlowContext Context;

    public abstract string Name { get; }
    public virtual bool CanSkipDay => false;

    protected GameFlowStateBase(IGameFlowContext context)
    {
        Context = context;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void OnTimerElapsed() { }
    public virtual void OnCharacterExecuted(CharacterAI victim) { }
}
