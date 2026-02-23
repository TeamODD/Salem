using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WitchAI : CharacterAI
{
    private CharacterAI currentLieTarget;
    private Role.Roles? myPersona; // 게임 내내 유지할 사칭 컨셉
    private HashSet<CharacterAI> refusedVisitors = new HashSet<CharacterAI>(); // 내가 거부한 방문자들

    public override CharacterAI CurrentLieTarget => currentLieTarget;
    public override bool ShouldIgnorePrayerDialogueOverride => true;

    // 마녀는 무조건 기도를 거부함
    public override bool WillRefusePrayer => true;

    public override bool TryGetReceivedPrayerForCitizenDialogue(out bool prayerReceived)
    {
        prayerReceived = false;
        return true;
    }

    public override void OnVisitorRefused(CharacterAI visitor)
    {
        refusedVisitors.Add(visitor);
        Debug.Log($"[Witch] {DisplayName} -> {visitor.DisplayName}의 방문을 거부함. (공격 금지 목록 추가)");
    }

    public override void DoNightAction(AIContext context)
    {
        currentLieTarget = null;
        AIActionType finalActionType = AIActionType.WitchAttack;

        // 1. 이번 밤에 죽일 대상 정하기
        CharacterAI attackTarget = ChooseAttackTarget(context, myPersona);

        // 2. 페르소나 결정 (첫날 밤 공격 대상을 정한 직후 수행)
        if (myPersona == null && context.NightIndex == 1)
        {
            DecidePersona(context, attackTarget);
        }

        // 3. 이번 밤에 연기할 역할 결정
        Role.Roles? pretend = DeterminePretendRole(context);

        string targetName = attackTarget != null ? attackTarget.DisplayName : "없음";
        string pretendName = pretend.HasValue ? pretend.Value.ToString() : "없음";
        Debug.Log($"[Witch] {DisplayName} -> 공격 대상: {targetName}, 사칭 직업: {pretendName}");

        if (pretend == Role.Roles.신자)
        {
            context.MarkWitchPretendedBeliever(this);
            finalActionType = DetermineLieTargetForBeliever(context, attackTarget);
        }
        else if (pretend == Role.Roles.불면증)
        {
            // 불면증 사칭: 홀수 날 집에 머물고 짝수 날 외출
            finalActionType = context.IsEvenNight() ? AIActionType.InsomniacWalk : AIActionType.InsomniacHome;
        }
        else if (pretend == Role.Roles.벙어리 || pretend == Role.Roles.겁쟁이)
        {
            finalActionType = AIActionType.MuteSilent;
        }
        else if (pretend == Role.Roles.시민)
        {
            finalActionType = AIActionType.CitizenHome;
        }
        else if (pretend == Role.Roles.좀도둑)
        {
            // 좀도둑 사칭: 빈집이 있을 때만 외출한 척
            finalActionType = context.HasEmptyHouseForThief ? AIActionType.ThiefLie : AIActionType.CitizenHome;
        }

        // 실제 공격 대상은 attackTarget이지만, ActionId는 거짓말에 맞춰서 설정
        SetAction(context, finalActionType, context.GetCharacter(attackTarget), pretendRole: pretend);

        if (attackTarget != null)
        {
            context.MarkAttacked(attackTarget);
        }
    }

    public override void ResolveMorning(AIContext context)
    {
        // 마녀는 시스템에서 처리하므로 추가 로직 불필요
    }

    private void DecidePersona(AIContext context, CharacterAI firstVictim)
    {
        // 첫날 밤 살해한 캐릭터가 신자일 경우 -> 페르소나 신자로 설정
        if (firstVictim != null && firstVictim.MyRole == Role.Roles.신자)
        {
            myPersona = Role.Roles.신자;
        }
        else
        {
            // 첫날 밤 살해한 캐릭터가 신자 이외일 경우 -> 50% 신자, 50% 다른 특성
            if (Random.value < 0.5f)
            {
                myPersona = Role.Roles.신자;
            }
            else
            {
                myPersona = PickRandomTrait(context);
            }
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

            // 3-1. 겁쟁이는 살해하지 않음
            if (ai.MyRole == Role.Roles.겁쟁이) continue;

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

    private AIActionType DetermineLieTargetForBeliever(AIContext context, CharacterAI actualAttackTarget)
    {
        // 20% 확률로 무작위 거짓말 (성공했다고 주장)
        if (Random.value < 0.2f)
        {
            DetermineLieTargetSimple(context, null);
            return AIActionType.BelieverInvestigate;
        }

        // 80% 확률로 논리적 거짓말
        // 우선순위 대상: 벙어리, 산책간 불면증, 도둑질나간 좀도둑, 겁쟁이
        List<(CharacterAI target, AIActionType actionType)> candidates = new List<(CharacterAI, AIActionType)>();

        foreach (CharacterAI p in context.Participants)
        {
            if (p == this) continue;

            if (p.MyRole == Role.Roles.벙어리)
            {
                candidates.Add((p, AIActionType.BelieverInvestigate));
            }
            else if (p.MyRole == Role.Roles.불면증 && context.IsEvenNight())
            {
                candidates.Add((p, AIActionType.BelieverAbsent));
            }
            else if (p.MyRole == Role.Roles.좀도둑 && context.HasEmptyHouseForThief)
            {
                candidates.Add((p, AIActionType.BelieverAbsent));
            }
            else if (p.MyRole == Role.Roles.겁쟁이)
            {
                candidates.Add((p, AIActionType.BelieverRefused));
            }
        }

        if (candidates.Count > 0)
        {
            var choice = candidates[Random.Range(0, candidates.Count)];
            currentLieTarget = choice.target;
            return choice.actionType;
        }

        // 위 조건에 맞는 대상이 없으면 오늘 죽인 사람을 대상으로 함 (시체 발견)
        currentLieTarget = actualAttackTarget;
        
        if (currentLieTarget == null)
        {
            DetermineLieTargetSimple(context, null);
            return AIActionType.BelieverInvestigate;
        }

        return AIActionType.BelieverBodyFound;
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
