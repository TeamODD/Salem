public interface IGameFlowState
{
    string Name { get; }
    bool CanSkipDay { get; }

    void Enter();
    void Exit();
    void OnTimerElapsed();
    void OnCharacterExecuted(CharacterAI victim);
}
