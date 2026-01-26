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
            SetAction(context, "believer_stay_home", target: null);
            return;
        }

        var target = candidates[Random.Range(0, candidates.Count)];
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
        // TODO: 조사 결과 저장/적용 로직 연결 지점
    }
}
