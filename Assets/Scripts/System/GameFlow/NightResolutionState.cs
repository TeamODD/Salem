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

        yield return Context.FadeInRoutine(Context.FadeDuration);
        Context.ChangeState(new DayState(Context));
    }
}
