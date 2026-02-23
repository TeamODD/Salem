using UnityEngine;

[System.Serializable]
public class AIAction
{
    public AIActionType ActionType { get; set; }
    public string ActionId => ActionType.ToActionIdString();
    public Character Target { get; private set; }
    public CharacterAI TargetAI { get; private set; }
    public Role.Roles? PretendRole { get; set; }
    public bool Success { get; set; }

    public AIAction(AIActionType actionType, Character target, Role.Roles? pretendRole, bool success)
    {
        ActionType = actionType;
        SetTarget(target);
        PretendRole = pretendRole;
        Success = success;
    }

    public AIAction(string actionId, Character target, Role.Roles? pretendRole, bool success)
    {
        ActionType = AIActionTypeExtensions.TryParseActionId(actionId, out AIActionType parsedType)
            ? parsedType
            : AIActionType.None;
        SetTarget(target);
        PretendRole = pretendRole;
        Success = success;
    }

    public void SetTarget(Character target)
    {
        Target = target;
        TargetAI = target != null ? target.GetComponent<CharacterAI>() : null;
    }

    public bool IsBelieverClaim()
    {
        return PretendRole == Role.Roles.신자 || ActionType.IsBelieverRelated();
    }
}
