using System.Collections.Generic;
using UnityEngine;

public class BelieverAI : CharacterAI
{
    private readonly HashSet<CharacterAI> investigated = new HashSet<CharacterAI>();

    public override void DoNightAction(AIContext context)
    {
        var candidates = new List<CharacterAI>();
        foreach (var ai in context.Participants)
        {
            if (ai == null || ai == this) continue;
            if (investigated.Contains(ai)) continue;
            if (context.WitchPretendedBelievers.Contains(ai)) continue;
            candidates.Add(ai);
        }

        if (candidates.Count == 0)
        {
            Debug.Log($"[Believer] {DisplayName} -> 조사 대상 없음 (집에 머무름)");
            SetAction(context, "believer_stay_home", target: null);
            return;
        }

        var target = candidates[Random.Range(0, candidates.Count)];
        Debug.Log($"[Believer] {DisplayName} -> 조사 대상: {target.DisplayName}");
        investigated.Add(target);
        SetAction(context, "believer_investigate", context.GetCharacter(target));
    }

    public override void RecordDialogue(AIContext context)
    {
        if (lastAction == null)
        {
            AddDialogue("believer_stay_home");
            return;
        }

        AddDialogue(lastAction.ActionId);
    }

    public override void ResolveMorning(AIContext context)
    {
        if (lastAction == null || lastAction.Target == null) return;

        // Character 컴포넌트에서 CharacterAI 컴포넌트를 가져옵니다.
        var targetAI = lastAction.Target.GetComponent<CharacterAI>();
        if (targetAI == null) return;

        bool isHome = context.IsTargetHome(targetAI);
        bool isDead = context.Attacked.Contains(targetAI);

        if (isDead)
        {
            // 시체 발견
            lastAction.Success = false;
            lastAction.ActionId = "believer_body_found";
        }
        else if (!isHome)
        {
            // 부재
            lastAction.Success = false;
            lastAction.ActionId = "believer_absent";
        }
        else
        {
            // 성공: 상대방에게 '기도 받음' 상태 부여
            lastAction.Success = true;
            context.PrayerReceived.Add(targetAI);
        }
    }
}
