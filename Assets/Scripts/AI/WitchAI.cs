using System.Collections.Generic;
using UnityEngine;


public class WitchAI : CharacterAI
{
    public override void DoNightAction(AIContext context)
    {
        Role.Roles? pretend = ChoosePretendRole(context);
        var target = ChooseAttackTarget(context, pretend);

        if (pretend == Role.Roles.신자)
        {
            context.WitchPretendedBelievers.Add(this);
        }

        SetAction(context, "witch_attack", context.GetCharacter(target), pretendRole: pretend);

        if (target != null)
        {
            context.Attacked.Add(target);
        }
    }

    public override void RecordDialogue(AIContext context)
    {
        AddDialogue("witch_pretend");
    }

    public override void ResolveMorning(AIContext context)
    {
        // TODO: 습격 결과 적용 지점
    }

    private Role.Roles? ChoosePretendRole(AIContext context)
    {
        List<Role.Roles> choices = new List<Role.Roles>();
        foreach (var roleItem in context.ActiveRoles)
        {
            if (roleItem == Role.Roles.마녀) continue;
            choices.Add(roleItem);
        }

        if (choices.Count == 0) return null;
        return choices[Random.Range(0, choices.Count)];
    }

    private CharacterAI ChooseAttackTarget(AIContext context, Role.Roles? pretendRole)
    {
        List<CharacterAI> candidates = new List<CharacterAI>();
        foreach (var ai in context.Participants)
        {
            if (ai == null || ai == this) continue;
            if (pretendRole.HasValue && ai.MyRole == pretendRole.Value) continue;
            candidates.Add(ai);
        }

        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }
}
