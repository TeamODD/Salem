using UnityEngine;

[System.Serializable]
public class AIAction
{
    public string ActionId;
    public Character Target;
    public Role.Roles? PretendRole;
    public bool Success;

    public AIAction(string actionId, Character target, Role.Roles? pretendRole, bool success)
    {
        ActionId = actionId;
        Target = target;
        PretendRole = pretendRole;
        Success = success;
    }
}
