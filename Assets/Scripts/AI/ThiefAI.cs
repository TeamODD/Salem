using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ThiefAI : CharacterAI
{
    private CharacterAI currentLieTarget;
    private Role.Roles? myPersona;
    private bool receivedPrayer = false;

    public override CharacterAI CurrentLieTarget => currentLieTarget;
    public override bool ShouldIgnorePrayerDialogueOverride => lastAction != null && lastAction.Success;
    
    // 시민인 척 할 때 기도 받았는지 여부 확인용
    public bool HasReceivedPrayer => receivedPrayer;

    public override bool WillRefusePrayer => myPersona == Role.Roles.겁쟁이;

    public override void DoNightAction(AIContext context)
    {
        // 페르소나 결정/갱신
        DecidePersona(context);

        currentLieTarget = null;
        AIActionType actionType = AIActionType.ThiefTruth;
        Role.Roles? pretend = null;
        CharacterAI target = null;
        bool success = false;

        if (context.HasEmptyHouseForThief)
        {
            // 도둑질 성공 -> 마녀처럼 페르소나 연기
            pretend = myPersona;
            success = true;
            
            string pretendName = pretend.HasValue ? pretend.Value.ToString() : "없음";
            
            // 누구의 집을 털었는지 확인 및 로그 출력
            List<CharacterAI> outOfHouse = context.GetOutOfHouse().Where(p => p != this).ToList();
            if (outOfHouse.Count > 0)
            {
                CharacterAI stolenTarget = outOfHouse[Random.Range(0, outOfHouse.Count)];
                Debug.Log($"[Thief] {DisplayName} -> {stolenTarget.DisplayName}의 빈집을 털었습니다! (사칭 모드: {pretendName})");
            }

            // 신자인 척 할 경우 가짜 조사 대상 선정 (마녀와 동일 로직)
            if (pretend == Role.Roles.신자)
            {
                actionType = DetermineLieTargetForBeliever(context);
            }
            else
            {
                actionType = AIActionType.ThiefLie; // 기본 성공/사칭 액션
                if (pretend == Role.Roles.불면증)
                {
                     // 불면증 사칭 시 액션 ID 조정 (CharacterInteraction에서 사용)
                     actionType = context.IsEvenNight() ? AIActionType.InsomniacWalk : AIActionType.InsomniacHome;
                }
                else if (pretend == Role.Roles.벙어리 || pretend == Role.Roles.겁쟁이)
                {
                    actionType = AIActionType.MuteSilent;
                }
            }
        }
        else
        {
            // 도둑질 실패 -> 시민처럼 행동
            Debug.Log($"[Thief] {DisplayName} -> 빈집털이 실패.. 시민 연기");
            pretend = Role.Roles.시민;
            actionType = AIActionType.CitizenHome;
            success = false;
        }

        SetAction(context, actionType, context.GetCharacter(target), pretendRole: pretend, success: success);
    }

    public override void ResolveMorning(AIContext context)
    {
        // 시민인 척 할 때 기도를 받았는지 확인
        if (lastAction != null && lastAction.PretendRole == Role.Roles.시민)
        {
            receivedPrayer = context.HasReceivedPrayer(this);
        }
    }

    public override bool TryGetReceivedPrayerForCitizenDialogue(out bool prayerReceived)
    {
        if (lastAction != null && !lastAction.Success)
        {
            prayerReceived = receivedPrayer;
            return true;
        }

        prayerReceived = false;
        return false;
    }

    private void DecidePersona(AIContext context)
    {
        // 1. 게임에 불면증이 존재할 경우 100% 불면증인 척 (이미 설정되었으면 유지)
        if (myPersona == Role.Roles.불면증) return;
        
        bool hasInsomniac = context.Participants.Any(p => p.MyRole == Role.Roles.불면증);
        if (hasInsomniac)
        {
            myPersona = Role.Roles.불면증;
            return;
        }

        // 2. 첫날 밤: 신자 사칭 여부를 결정할 수 없으므로 다른 것 선택
        if (context.NightIndex == 1)
        {
            if (myPersona == null)
            {
                List<Role.Roles> traits = new List<Role.Roles> { Role.Roles.겁쟁이, Role.Roles.벙어리 };
                myPersona = traits[Random.Range(0, traits.Count)];
            }
        }
        else if (context.NightIndex == 2 && (myPersona == Role.Roles.겁쟁이 || myPersona == Role.Roles.벙어리))
        {
            // 3. 둘째 날: 첫날에 신자가 살해당하지 않았을 경우 50% 확률로 신자로 전환
            bool believerKilledFirstNight = context.DeadParticipants.Any(p => p.MyRole == Role.Roles.신자);
            bool believerAlive = context.Participants.Any(p => p.MyRole == Role.Roles.신자);

            if (believerAlive && !believerKilledFirstNight)
            {
                if (Random.value < 0.5f)
                {
                    myPersona = Role.Roles.신자;
                    Debug.Log($"[Thief] {DisplayName} -> 신자가 무사하므로 페르소나를 신자로 변경합니다.");
                }
            }
        }
        
        if (context.NightIndex == 1)
            Debug.Log($"[Thief] {DisplayName} -> 이번 밤 페르소나: {myPersona}");
    }

    private AIActionType DetermineLieTargetForBeliever(AIContext context)
    {
        // 20% 확률로 무작위 거짓말 (성공했다고 주장)
        if (Random.value < 0.2f)
        {
            PickRandomLieTarget(context);
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
                // 다른 좀도둑이 있을 경우
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

        // 위 조건에 맞는 대상이 없으면 랜덤 (시체는 좀도둑이 안 죽였으므로 모름 -> 그냥 랜덤 성공)
        PickRandomLieTarget(context);
        return AIActionType.BelieverInvestigate;
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
