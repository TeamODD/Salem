using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ThiefAI : CharacterAI
{
    private CharacterAI currentLieTarget;
    public override CharacterAI CurrentLieTarget => currentLieTarget;

    public override void DoNightAction(AIContext context)
    {
        currentLieTarget = null;

        if (context.HasEmptyHouseForThief)
        {
            Role.Roles? pretend = ChoosePretendRole(context);
            string pretendName = pretend.HasValue ? pretend.Value.ToString() : "없음";
            Debug.Log($"[Thief] {DisplayName} -> 빈집털이 시도 (사칭: {pretendName})");
            
            // 신자인 척 할 경우 가짜 조사 대상 선정
            if (pretend == Role.Roles.신자)
            {
                PickRandomLieTarget(context);
            }

            SetAction(context, "thief_lie", target: null, pretendRole: pretend);
        }
        else
        {
            SetAction(context, "thief_truth", target: null);
        }
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
        foreach (Role.Roles roleItem in context.ActiveRoles)
        {
            if (roleItem == Role.Roles.좀도둑) continue;
            choices.Add(roleItem);
        }

        if (choices.Count == 0) return null;
        return choices[Random.Range(0, choices.Count)];
    }

    private void PickRandomLieTarget(AIContext context)
    {
        List<CharacterAI> others = context.Participants.Where(p => p != this).ToList();
        if (others.Count > 0)
        {
            currentLieTarget = others[Random.Range(0, others.Count)];
        }
    }
}