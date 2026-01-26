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
        // TODO: 도둑질 성공/실패 결과 적용 지점
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
