using System.Collections;

public interface IGameFlowContext
{
    float FadeDuration { get; }
    float IntroLeadDelay { get; }
    float NightResolveDelay { get; }
    int CurrentLevelIndex { get; }

    void ChangeState(IGameFlowState nextState);
    void StartManagedRoutine(IEnumerator routine);
    void StopManagedRoutine();

    void SetTransitioning(bool value);
    void SetNight(bool value);
    void StopTimer();
    void ResetTimerPaused();
    void ResetTimer();
    void ReduceRoundTimeForNextRound();
    void SetFadeOpaque();

    void RunNight();
    void RunMorning();

    bool TryApplyExecution(CharacterAI victim);
    bool IsWinConditionMet();
    bool IsDefeatConditionMet();
    IEnumerator ShowDefeatUIRoutine();
    void RecordRoundEndMetrics();
    void FinalizeScoreAndOpenResult(bool isVictory);
    IEnumerator ShowNightDeathNoticeRoutine();

    IEnumerator FadeOutRoutine(float duration);
    IEnumerator FadeInRoutine(float duration);
    IEnumerator LoadLevelRoutine(int levelIndex);
}
