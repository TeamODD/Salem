using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterAI : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string displayName;

    [Header("Role")]
    [SerializeField] protected Role.Roles role;

    protected AIAction lastAction;

    public Role.Roles MyRole => role;
    public AIAction LastAction => lastAction;
    public string DisplayName => string.IsNullOrEmpty(displayName) ? gameObject.name : displayName;
    public virtual CharacterAI CurrentLieTarget => null;
    public virtual bool WillRefusePrayer => false;

    public abstract void DoNightAction(AIContext context);
    public abstract void ResolveMorning(AIContext context);

    protected void SetAction(AIContext context, string actionId, Character target = null, Role.Roles? pretendRole = null, bool success = true)
    {
        lastAction = new AIAction(actionId, target, pretendRole, success);
        if (context != null)
        {
            context.RegisterAction(this, lastAction);
        }
    }

    public virtual void OnVisitorRefused(CharacterAI visitor) { }

    public void SetDisplayName(string newName)
    {
        displayName = newName;
    }

    public void SetRole(Role.Roles newRole)
    {
        role = newRole;
    }

    public void Initialize(Role.Roles assignedRole)
    {
        role = assignedRole;
    }
}