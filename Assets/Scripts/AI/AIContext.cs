using System.Collections.Generic;
using UnityEngine;

public class AIContext
{
    private readonly List<CharacterAI> _participants = new List<CharacterAI>();
    private readonly List<CharacterAI> _deadParticipants = new List<CharacterAI>();
    private readonly List<Role.Roles> _activeRoles = new List<Role.Roles>();
    private readonly Dictionary<CharacterAI, AIAction> _actions = new Dictionary<CharacterAI, AIAction>();
    private readonly Dictionary<CharacterAI, List<CharacterAI>> _actorsByTarget = new Dictionary<CharacterAI, List<CharacterAI>>();
    private readonly HashSet<CharacterAI> _attacked = new HashSet<CharacterAI>();
    private readonly HashSet<CharacterAI> _witchPretendedBelievers = new HashSet<CharacterAI>();
    private readonly HashSet<CharacterAI> _prayerReceived = new HashSet<CharacterAI>();
    private readonly HashSet<CharacterAI> _outOfHouse = new HashSet<CharacterAI>();

    public int NightIndex { get; }
    public bool HasEmptyHouseForThief { get; }
    public IReadOnlyList<CharacterAI> Participants => _participants;
    public IReadOnlyList<CharacterAI> DeadParticipants => _deadParticipants;
    public IReadOnlyList<Role.Roles> ActiveRoles => _activeRoles;
    public IReadOnlyDictionary<CharacterAI, AIAction> Actions => _actions;
    public IReadOnlyCollection<CharacterAI> Attacked => _attacked;

    public AIContext(
        int nightIndex,
        bool hasEmptyHouseForThief,
        IEnumerable<CharacterAI> participants,
        IEnumerable<CharacterAI> deadParticipants,
        IEnumerable<Role.Roles> activeRoles)
    {
        NightIndex = nightIndex;
        HasEmptyHouseForThief = hasEmptyHouseForThief;

        if (participants != null) _participants.AddRange(participants);
        if (deadParticipants != null) _deadParticipants.AddRange(deadParticipants);
        if (activeRoles != null) _activeRoles.AddRange(activeRoles);
    }

    public bool IsTargetHome(CharacterAI target)
    {
        return target != null && !_outOfHouse.Contains(target);
    }

    public void RegisterAction(CharacterAI actor, AIAction action)
    {
        if (actor == null || action == null) return;

        _actions[actor] = action;
        CharacterAI target = action.TargetAI;
        if (target == null) return;

        if (!_actorsByTarget.TryGetValue(target, out List<CharacterAI> actors))
        {
            actors = new List<CharacterAI>();
            _actorsByTarget[target] = actors;
        }

        if (!actors.Contains(actor))
        {
            actors.Add(actor);
        }
    }

    public bool IsEvenNight()
    {
        return NightIndex % 2 == 0;
    }

    public List<CharacterAI> GetParticipantsByRole(Role.Roles role)
    {
        List<CharacterAI> result = new List<CharacterAI>();
        foreach (CharacterAI ai in _participants)
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

    public void MarkOutOfHouse(CharacterAI ai)
    {
        if (ai != null) _outOfHouse.Add(ai);
    }

    public void MarkAttacked(CharacterAI ai)
    {
        if (ai != null) _attacked.Add(ai);
    }

    public void MarkWitchPretendedBeliever(CharacterAI ai)
    {
        if (ai != null) _witchPretendedBelievers.Add(ai);
    }

    public void MarkPrayerReceived(CharacterAI ai)
    {
        if (ai != null) _prayerReceived.Add(ai);
    }

    public bool HasReceivedPrayer(CharacterAI ai)
    {
        return ai != null && _prayerReceived.Contains(ai);
    }

    public bool IsWitchPretendedBeliever(CharacterAI ai)
    {
        return ai != null && _witchPretendedBelievers.Contains(ai);
    }

    public bool IsAttacked(CharacterAI ai)
    {
        return ai != null && _attacked.Contains(ai);
    }

    public IEnumerable<CharacterAI> GetOutOfHouse()
    {
        return _outOfHouse;
    }

    public bool TryGetSuccessfulBelieverVisitorName(CharacterAI target, out string visitorName)
    {
        visitorName = null;
        if (target == null) return false;
        if (!_actorsByTarget.TryGetValue(target, out List<CharacterAI> actors)) return false;

        foreach (CharacterAI actor in actors)
        {
            if (actor == null) continue;
            if (!_actions.TryGetValue(actor, out AIAction action) || action == null) continue;
            if (!action.Success) continue;

            bool isBelieverRole = actor.MyRole == Role.Roles.신자;
            if (!isBelieverRole && !action.IsBelieverClaim()) continue;

            visitorName = actor.DisplayName;
            return true;
        }

        return false;
    }
}
