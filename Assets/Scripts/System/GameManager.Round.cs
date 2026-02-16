using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class GameManager
{
    public void AssignRandomRoles()
    {
        participants.Clear();
        deadParticipants.Clear();

        List<CharacterAI> newParticipants = roleAssigner.AssignRoles(characterObjects, activeRoles);
        participants.AddRange(newParticipants);
        Debug.Log($"<color=green>모든 캐릭터에게 새로운 직업이 부여되었습니다. (참가자: {participants.Count}명)</color>");
    }

    public void RunNight()
    {
        bool isEvenNight = nightIndex % 2 == 0;
        bool believerInvestigating = participants.Exists(ai => ai is BelieverAI);
        bool insomniacWalking = isEvenNight && participants.Exists(ai => ai is InsomniacAI);
        hasEmptyHouseForThief = insomniacWalking || believerInvestigating;

        BuildContext();

        foreach (CharacterAI ai in participants)
        {
            if (ai == null) continue;
            if (ai is BelieverAI) currentContext.OutOfHouse.Add(ai);
            if (ai is InsomniacAI && isEvenNight) currentContext.OutOfHouse.Add(ai);
            if (ai is ThiefAI && hasEmptyHouseForThief) currentContext.OutOfHouse.Add(ai);
        }

        foreach (CharacterAI ai in participants)
        {
            if (ai != null) ai.DoNightAction(currentContext);
        }
    }

    public void RunMorning()
    {
        if (currentContext == null) BuildContext();

        foreach (CharacterAI ai in participants)
        {
            if (ai is BelieverAI) ai.ResolveMorning(currentContext);
        }

        foreach (CharacterAI ai in participants)
        {
            if (!(ai is BelieverAI)) ai.ResolveMorning(currentContext);
        }

        foreach (CharacterAI victim in currentContext.Attacked)
        {
            if (victim != null && participants.Contains(victim))
            {
                Debug.Log($"[Night Event] {victim.DisplayName} 사망.");
                victim.gameObject.SetActive(false);
                participants.Remove(victim);
                deadParticipants.Add(victim);
            }
        }

        nightIndex++;
    }

    public bool TryApplyExecution(CharacterAI victim)
    {
        if (victim == null || isTransitioning) return false;

        if (participants.Contains(victim))
        {
            participants.Remove(victim);
            deadParticipants.Add(victim);
            Debug.Log($"[GameManager] {victim.DisplayName} 처형됨.");
        }

        victim.gameObject.SetActive(false);
        return true;
    }

    public bool IsWinConditionMet()
    {
        if (participants.Count == 0) return false;

        participants.RemoveAll(ai => ai == null || !ai.gameObject.activeInHierarchy);
        bool witchAlive = participants.Exists(ai => ai.MyRole == Role.Roles.마녀);
        return !witchAlive;
    }

    public bool IsDefeatConditionMet()
    {
        participants.RemoveAll(ai => ai == null || !ai.gameObject.activeInHierarchy);

        int deadCivilians = deadParticipants.Count(dead => dead.MyRole != Role.Roles.마녀);
        bool isBulletEmpty = ExecutionManager.Instance != null && ExecutionManager.Instance.CurrentBullets <= 0;
        bool isTooManyDead = deadCivilians >= 3;

        if (!isBulletEmpty && !isTooManyDead) return false;

        bool witchAlive = participants.Exists(ai => ai.MyRole == Role.Roles.마녀);
        return witchAlive;
    }

    private void BuildContext()
    {
        currentContext = new AIContext
        {
            NightIndex = nightIndex,
            HasEmptyHouseForThief = hasEmptyHouseForThief
        };
        currentContext.Participants.AddRange(participants);
        currentContext.DeadParticipants.AddRange(deadParticipants);
        currentContext.ActiveRoles.AddRange(activeRoles);
    }
}
