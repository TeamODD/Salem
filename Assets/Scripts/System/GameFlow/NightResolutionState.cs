using System.Collections;
using UnityEngine;

public sealed class NightResolutionState : GameFlowStateBase
{
    public override string Name => "NightResolution";

    public NightResolutionState(IGameFlowContext context) : base(context)
    {
    }

    public override void Enter()
    {
        Context.SetTransitioning(true);
        Context.SetNight(true);
        Context.StopTimer();
        Context.StartManagedRoutine(RunNightFlow());
    }

    public override void Exit()
    {
        Context.StopManagedRoutine();
    }

    private IEnumerator RunNightFlow()
    {
        if (Context.IsDefeatConditionMet())
        {
            Context.ChangeState(new GameOverState(Context));
            yield break;
        }

        Debug.Log("--- 밤이 시작되었습니다 ---");
        yield return Context.FadeOutRoutine(Context.FadeDuration);

        Context.RunNight();
        yield return new WaitForSeconds(Context.NightResolveDelay);
        Context.RunMorning();
        yield return Context.ShowNightDeathNoticeRoutine();

        Debug.Log("--- 아침이 밝았습니다 ---");

        if (Context.IsWinConditionMet())
        {
            Context.ChangeState(new LevelTransitionState(Context));
            yield break;
        }

        if (Context.IsDefeatConditionMet())
        {
            Context.ChangeState(new GameOverState(Context));
            yield break;
        }

        // 다음날 화면이 보이기 전에 타이머 UI만 비우고, 낮 시작 전까지는 정지 상태를 유지한다.
        Context.ResetTimerPaused();
        yield return Context.FadeInRoutine(Context.FadeDuration);
        Context.ChangeState(new DayState(Context));
    }
}
