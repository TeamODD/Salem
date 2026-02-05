using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WitchAI : CharacterAI
{
    private CharacterAI currentLieTarget;
    public override CharacterAI CurrentLieTarget => currentLieTarget;

    public override void DoNightAction(AIContext context)
    {
        currentLieTarget = null;
        Role.Roles? pretend = DeterminePretendRole(context);
        var attackTarget = ChooseAttackTarget(context, pretend);

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
            // 다른 직업인 척 할 때는 해당 직업 행동에 맞는 거짓말 대상 선정 (간단히 랜덤 or 공격대상)
            // 여기서는 공격 대상을 그대로 사용하거나 랜덤
            DetermineLieTargetSimple(context, attackTarget);
        }

        SetAction(context, "witch_attack", context.GetCharacter(attackTarget), pretendRole: pretend);

        if (attackTarget != null)
        {
            context.Attacked.Add(attackTarget);
        }
    }

    public override void RecordDialogue(AIContext context)
    {
        // 커스텀 다이얼로그 처리 (거짓말 대상 적용을 위해)
        string actionId = "witch_pretend";

        if (dialogueLibrary == null) return;

        string line = dialogueLibrary.GetRandomLine(role, actionId);
        if (string.IsNullOrEmpty(line)) return;

        string myName = gameObject.name; // DisplayName이 없으면 gameObject.name
                                         // 부모 클래스의 DisplayName 접근이 어렵다면 GetComponent로 확인
                                         // (CharacterAI의 displayName이 serializedField라 직접 접근 가능할 수도 있으나 protected임)
                                         // 여기선 간단히 name 사용. 필요시 protected 필드 활용.

        line = line.Replace("{Name}", myName);

        if (currentLieTarget != null)
        {
            // 대상 이름 가져오기
            string targetName = currentLieTarget.name;
            // CharacterAI 컴포넌트 찾기 시도
            // (CharacterAI는 MonoBehaviour이므로 null 체크)
            if (currentLieTarget != null)
            {
                // Reflection이나 protected 필드 접근이 안되므로 name 사용
                targetName = currentLieTarget.name;
            }
            line = line.Replace("{Target}", targetName);
        }
        else
        {
            line = line.Replace("{Target}", "누군가");
        }

        if (lastAction != null && lastAction.PretendRole.HasValue)
        {
            line = line.Replace("{PretendRole}", lastAction.PretendRole.Value.ToString());
        }

        nightDialogues.Add(line);
    }

    public override void ResolveMorning(AIContext context)
    {
        // 마녀는 아침 결과 처리가 특별히 필요 없음 (습격은 시스템에서 처리)
    }

    private Role.Roles? DeterminePretendRole(AIContext context)
    {
        // 3-2. 만약 처음으로 사망한 사람이 신자라면, 마녀는 반드시 신자인 척을 함.
        if (context.DeadParticipants.Count > 0)
        {
            var firstDead = context.DeadParticipants[0];
            if (firstDead != null && firstDead.MyRole == Role.Roles.신자)
            {
                return Role.Roles.신자;
            }
        }

        // 3-3. 신자가 살아있다면...
        bool believerAlive = context.Participants.Any(p => p.MyRole == Role.Roles.신자);

        if (believerAlive)
        {
            // 50% 확률로 신자인 척
            if (Random.value < 0.5f)
            {
                return Role.Roles.신자;
            }
            else
            {
                // 50% 확률로 다른 직업인 척 (시민 포함 5개 특성 중 랜덤 -> 여기선 Enum에 있는 나머지 특성 중 선택)
                return PickRandomTrait(context);
            }
        }

        // 그 외 (신자가 죽었지만 첫 번째가 아닌 경우 등): 랜덤
        // 규칙에 명시되지 않았으므로 신자 포함 전체 랜덤 or 특성 랜덤
        // "신자가 살아있다면" 조건 외의 경우이므로 자유. 여기선 전체 랜덤 사용.
        return PickRandomRole(context);
    }

    private Role.Roles PickRandomTrait(AIContext context)
    {
        // 시민 제외 4개 특성: 좀도둑, 불면증, 겁쟁이, 벙어리
        // 시민은 Enum에 없으므로 제외
        var traits = new List<Role.Roles> { Role.Roles.좀도둑, Role.Roles.불면증, Role.Roles.겁쟁이, Role.Roles.벙어리 };
        return traits[Random.Range(0, traits.Count)];
    }

    private Role.Roles PickRandomRole(AIContext context)
    {
        // 마녀 제외 모든 역할 중 랜덤
        var choices = new List<Role.Roles>();
        foreach (Role.Roles r in System.Enum.GetValues(typeof(Role.Roles)))
        {
            if (r != Role.Roles.마녀)
            {
                choices.Add(r);
            }
        }
        return choices[Random.Range(0, choices.Count)];
    }

    private CharacterAI ChooseAttackTarget(AIContext context, Role.Roles? pretendRole)
    {
        List<CharacterAI> candidates = new List<CharacterAI>();
        foreach (var ai in context.Participants)
        {
            if (ai == null || ai == this) continue;

            // 3-3. 신자인 척을 했다면 밤에 신자를 공격해선 안 됨.
            if (pretendRole == Role.Roles.신자 && ai.MyRole == Role.Roles.신자) continue;

            candidates.Add(ai);
        }

        if (candidates.Count == 0) return null;

        // 3-1. 첫 날 죽이는 직업은 완전 랜덤 (이미 candidates가 필터링 되었으므로 랜덤 선택)
        return candidates[Random.Range(0, candidates.Count)];
    }

    private void DetermineLieTargetForBeliever(AIContext context, CharacterAI actualAttackTarget)
    {
        // 3-2. 20%확률로 이를 모두 무시하고 그냥 단순하게 랜덤으로 한 명과 기도했다고 거짓말을 함.
        if (Random.value < 0.2f)
        {
            DetermineLieTargetSimple(context, null);
            return;
        }

        // 우선순위: 벙어리, 산책을 간 불면증, 도둑질을 하러 나간 좀도둑, 겁쟁이
        var priorities = new List<CharacterAI>();

        foreach (var p in context.Participants)
        {
            if (p == this) continue;

            bool matches = false;

            if (p.MyRole == Role.Roles.벙어리) matches = true;
            else if (p.MyRole == Role.Roles.겁쟁이) matches = true;
            else if (p.MyRole == Role.Roles.불면증)
            {
                // 짝수 날 밤에 산책
                if (context.IsEvenNight()) matches = true;
            }
            else if (p.MyRole == Role.Roles.좀도둑)
            {
                // 빈집이 있으면 도둑질
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
            // 만약 이 중 아무도 없다면 한 번 자신이 죽인 사람을 조사함.
            // 죽인 사람이 없다면(공격 실패/스킵 등), 랜덤 대상을 지목
            currentLieTarget = actualAttackTarget;
            if (currentLieTarget == null)
            {
                 DetermineLieTargetSimple(context, null);
            }
        }
    }

    private void DetermineLieTargetSimple(AIContext context, CharacterAI defaultTarget)
    {
        // 랜덤으로 한 명 선택 (자신 제외)
        var others = context.Participants.Where(p => p != this).ToList();
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