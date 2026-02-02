using System.Collections.Generic;
using UnityEngine;

public class ThiefAI : CharacterAI
{
    public override void DoNightAction(AIContext context)
    {
        if (context.HasEmptyHouseForThief)
        {
            Role.Roles? pretend = ChoosePretendRole(context);
            SetAction(context, "thief_lie", target: null, pretendRole: pretend);
        }
        else
        {
            SetAction(context, "thief_truth", target: null);
        }
    }

    public override void RecordDialogue(AIContext context)
    {
        if (lastAction == null)
        {
            AddDialogue("thief_truth");
            return;
        }

        AddDialogue(lastAction.ActionId);
    }

    public override void ResolveMorning(AIContext context)
    {
        if (lastAction == null) return;

        // 좀도둑은 밤 행동 시점에 이미 성공 여부(빈 집 존재 여부)를 알고 액션을 결정함.
        // 여기서는 액션 결과에 따른 성공 상태를 확정함.
        lastAction.Success = lastAction.ActionId == "thief_lie";
    }

    private Role.Roles? ChoosePretendRole(AIContext context)
    {
        List<Role.Roles> choices = new List<Role.Roles>();
        foreach (var roleItem in context.ActiveRoles)
        {
            if (roleItem == Role.Roles.좀도둑) continue;
            choices.Add(roleItem);
        }

        if (choices.Count == 0) return null;
        return choices[Random.Range(0, choices.Count)];
    }
}
