using System.Collections.Generic;
using UnityEngine;

public class AIContext
{
    public int NightIndex;
    public List<CharacterAI> Participants = new List<CharacterAI>();
    public List<Role.Roles> ActiveRoles = new List<Role.Roles>();

    public Dictionary<CharacterAI, AIAction> Actions = new Dictionary<CharacterAI, AIAction>();
    public HashSet<CharacterAI> Attacked = new HashSet<CharacterAI>();
    public HashSet<CharacterAI> WitchPretendedBelievers = new HashSet<CharacterAI>();
    public HashSet<CharacterAI> PrayerReceived = new HashSet<CharacterAI>();
    public HashSet<CharacterAI> OutOfHouse = new HashSet<CharacterAI>(); // 집을 비운 인원들

    public bool HasEmptyHouseForThief;

    public bool IsTargetHome(CharacterAI target)
    {
        return !OutOfHouse.Contains(target);
    }

    public void RegisterAction(CharacterAI actor, AIAction action)
    {
        if (actor == null || action == null) return;
        Actions[actor] = action;
    }

    public bool IsEvenNight()
    {
        return NightIndex % 2 == 0;
    }

    public List<CharacterAI> GetParticipantsByRole(Role.Roles role)
    {
        List<CharacterAI> result = new List<CharacterAI>();
        foreach (var ai in Participants)
        {
            if (ai != null && ai.MyRole == role)
            {
                result.Add(ai);
            }
        }
        return result;
    }

    public Character GetCharacter(CharacterAI ai)
    {
        if (ai == null) return null;
        return ai.GetComponent<Character>();
    }
}
