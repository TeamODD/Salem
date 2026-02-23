using System.Collections.Generic;
using UnityEngine;

public class BelieverAI : CharacterAI
{
    private readonly HashSet<CharacterAI> investigated = new HashSet<CharacterAI>();

    public override void DoNightAction(AIContext context)
    {
        List<CharacterAI> candidates = new List<CharacterAI>();
        foreach (CharacterAI ai in context.Participants)
        {
            if (ai == null || ai == this) continue;
            if (investigated.Contains(ai)) continue;
            if (context.IsWitchPretendedBeliever(ai)) continue;
            candidates.Add(ai);
        }

        if (candidates.Count == 0)
        {
            Debug.Log($"[Believer] {DisplayName} -> 조사 대상 없음 (집에 머무름)");
            SetAction(context, AIActionType.BelieverStayHome, target: null);
            return;
        }

        CharacterAI target = candidates[Random.Range(0, candidates.Count)];
        Debug.Log($"[Believer] {DisplayName} -> 조사 대상: {target.DisplayName}");
        investigated.Add(target);
        SetAction(context, AIActionType.BelieverInvestigate, context.GetCharacter(target));
    }

    public override void ResolveMorning(AIContext context)
    {
        if (lastAction == null || lastAction.Target == null) return;

        CharacterAI targetAI = lastAction.Target.GetComponent<CharacterAI>();
        if (targetAI == null) return;

        bool isHome = context.IsTargetHome(targetAI);
        bool isDead = context.IsAttacked(targetAI);

        if (isDead)
        {
            // 시체 발견
            lastAction.Success = false;
            lastAction.ActionType = AIActionType.BelieverBodyFound;
        }
        else if (!isHome)
        {
            // 부재
            lastAction.Success = false;
            lastAction.ActionType = AIActionType.BelieverAbsent;
        }
        else if (targetAI.WillRefusePrayer)
        {
            // 거부
            lastAction.Success = false;
            lastAction.ActionType = AIActionType.BelieverRefused;
            targetAI.OnVisitorRefused(this); // 거부 사실 통보
        }
        else
        {
            // 성공
            lastAction.Success = true;
            context.MarkPrayerReceived(targetAI);
        }
    }
}
