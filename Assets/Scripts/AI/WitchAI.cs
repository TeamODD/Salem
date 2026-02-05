using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WitchAI : CharacterAI
{
    private CharacterAI currentLieTarget;
    private Role.Roles? myPersona; // 게임 내내 유지할 사칭 컨셉
    private HashSet<CharacterAI> refusedVisitors = new HashSet<CharacterAI>(); // 내가 거부한 방문자들

    public override CharacterAI CurrentLieTarget => currentLieTarget;

    // 마녀는 30% 확률로 기도를 거부함
    public override bool WillRefusePrayer => Random.value < 0.3f;

    public override void OnVisitorRefused(CharacterAI visitor)
    {
        refusedVisitors.Add(visitor);
        Debug.Log($"[Witch] {DisplayName} -> {visitor.DisplayName}의 방문을 거부함. (공격 금지 목록 추가)");
    }

    public override void DoNightAction(AIContext context)
    {
        // 1. 게임 시작 시(또는 첫 밤) 컨셉 정하기
        if (myPersona == null)
        {
            DecidePersona(context);
        }

        currentLieTarget = null;

        // 2. 이번 밤에 연기할 역할 결정 (기본적으로 페르소나를 따르나, 상황에 따라 강제됨)
        Role.Roles? pretend = DeterminePretendRole(context);

        // 3. 행동 대상 정하기
        CharacterAI attackTarget = ChooseAttackTarget(context, pretend);

        // 3-2 예외 처리: 만약 이번 공격으로 첫 번째 사망자가 신자가 된다면, 즉시 신자 흉내로 전환
        if (context.DeadParticipants.Count == 0 && attackTarget != null && attackTarget.MyRole == Role.Roles.신자)
        {
            pretend = Role.Roles.신자;
            // 페르소나도 업데이트하여 이후 게임에서도 계속 신자 흉내를 내도록 함
            myPersona = Role.Roles.신자;
            Debug.Log($"[Witch] {DisplayName} -> 첫 희생자가 신자({attackTarget.DisplayName})이므로 사칭 직업을 신자로 변경합니다.");
        }

        string targetName = attackTarget != null ? attackTarget.DisplayName : "없음";
        string pretendName = pretend.HasValue ? pretend.Value.ToString() : "없음";
        Debug.Log($"[Witch] {DisplayName} -> 공격 대상: {targetName}, 사칭 직업: {pretendName}");

        if (pretend == Role.Roles.신자)
        {
            context.WitchPretendedBelievers.Add(this);
            DetermineLieTargetForBeliever(context, attackTarget);
        }
        else
        {
            DetermineLieTargetSimple(context, attackTarget);
        }

        SetAction(context, "witch_attack", context.GetCharacter(attackTarget), pretendRole: pretend);

        if (attackTarget != null)
        {
            context.Attacked.Add(attackTarget);
        }
    }

    public override void ResolveMorning(AIContext context)
    {
        // 마녀는 시스템에서 처리하므로 추가 로직 불필요
    }

    private void DecidePersona(AIContext context)
    {
        // 3-3. 신자가 살아있다면 50% 확률로 신자 컨셉, 50% 확률로 다른 직업 컨셉
        bool believerAlive = context.Participants.Any(p => p.MyRole == Role.Roles.신자);

        if (believerAlive)
        {
            if (Random.value < 0.5f)
            {
                myPersona = Role.Roles.신자;
            }
            else
            {
                myPersona = PickRandomTrait(context);
            }
        }
        else
        {
            // 신자가 없다면 그냥 다른 직업 중 하나 선택
            myPersona = PickRandomTrait(context);
        }

        Debug.Log($"[Witch] {DisplayName} -> 이번 게임 페르소나 확정: {myPersona}");
    }

    private Role.Roles? DeterminePretendRole(AIContext context)
    {
        // 3-2. 첫 사망자가 신자라면 반드시 신자 흉내 (페르소나 무시)
        if (context.DeadParticipants.Count > 0)
        {
            CharacterAI firstDead = context.DeadParticipants[0];
            if (firstDead != null && firstDead.MyRole == Role.Roles.신자)
            {
                return Role.Roles.신자;
            }
        }

        // 그 외의 경우 정해둔 페르소나 유지
        return myPersona;
    }

    private Role.Roles PickRandomTrait(AIContext context)
    {
        List<Role.Roles> traits = new List<Role.Roles> { Role.Roles.좀도둑, Role.Roles.불면증, Role.Roles.겁쟁이, Role.Roles.벙어리 };
        return traits[Random.Range(0, traits.Count)];
    }

    private CharacterAI ChooseAttackTarget(AIContext context, Role.Roles? pretendRole)
    {
        List<CharacterAI> candidates = new List<CharacterAI>();
        foreach (CharacterAI ai in context.Participants)
        {
            if (ai == null || ai == this) continue;

            // 3-1. 첫 날은 완전 랜덤 (필터링 무시)
            if (context.NightIndex > 1)
            {
                // 3-3. 신자 흉내 중이라면 신자 공격 금지
                if (pretendRole == Role.Roles.신자 && ai.MyRole == Role.Roles.신자) continue;

                // 3-3. 기도를 거부당했다면 해당 신자 공격 금지
                if (refusedVisitors.Contains(ai)) continue;
            }

            candidates.Add(ai);
        }

        if (candidates.Count == 0) return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private void DetermineLieTargetForBeliever(AIContext context, CharacterAI actualAttackTarget)
    {
        if (Random.value < 0.2f)
        {
            DetermineLieTargetSimple(context, null);
            return;
        }

        List<CharacterAI> priorities = new List<CharacterAI>();

        foreach (CharacterAI p in context.Participants)
        {
            if (p == this) continue;

            bool matches = false;

            if (p.MyRole == Role.Roles.벙어리) matches = true;
            else if (p.MyRole == Role.Roles.겁쟁이) matches = true;
            else if (p.MyRole == Role.Roles.불면증)
            {
                if (context.IsEvenNight()) matches = true;
            }
            else if (p.MyRole == Role.Roles.좀도둑)
            {
                if (context.HasEmptyHouseForThief) matches = true;
            }

            if (matches) priorities.Add(p);
        }

        if (priorities.Count > 0)
        {
            currentLieTarget = priorities[Random.Range(0, priorities.Count)];
        }
        else
        {
            currentLieTarget = actualAttackTarget;
            if (currentLieTarget == null)
            {
                DetermineLieTargetSimple(context, null);
            }
        }
    }

    private void DetermineLieTargetSimple(AIContext context, CharacterAI defaultTarget)
    {
        List<CharacterAI> others = context.Participants.Where(p => p != this).ToList();
        if (others.Count > 0)
        {
            currentLieTarget = others[Random.Range(0, others.Count)];
        }
        else
        {
            currentLieTarget = defaultTarget;
        }
    }
}