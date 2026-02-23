using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public interface ICharacterDialogueVariableBinder
{
    void BindDefaultTargetName(DialogueRunner dialogueRunner, CharacterAI ai);
    void SetTargetName(DialogueRunner dialogueRunner, string targetName);
}

public interface ICharacterDialogueNodeResolver
{
    DialogueNodeResolution Resolve(CharacterAI ai, string baseNode, AIContext context);
}

public interface IExecutionClickHandler
{
    bool TryHandleClick(
        CharacterAI ai,
        CharacterData characterData,
        DialogueRunner dialogueRunner,
        CharacterVisual visual,
        CharacterExecutionState state);
}

public interface ICharacterFocusController
{
    bool IsMouseOver { get; }
    bool IsFocusLocked { get; }
    void OnPointerEnter(bool isDialogueRunning);
    void OnPointerExit();
    void LockFocus();
    void ReleaseFocusLock();
}

public readonly struct DialogueNodeResolution
{
    public string NodeName { get; }
    public string TargetNameOverride { get; }
    public bool HasTargetNameOverride => !string.IsNullOrEmpty(TargetNameOverride);

    public DialogueNodeResolution(string nodeName, string targetNameOverride)
    {
        NodeName = nodeName;
        TargetNameOverride = targetNameOverride;
    }
}

public sealed class CharacterExecutionState
{
    public bool HasShownExecutionDialogue;
}

public sealed class CharacterFocusController : ICharacterFocusController
{
    private readonly CharacterVisual _visual;
    public bool IsMouseOver { get; private set; }
    public bool IsFocusLocked { get; private set; }

    public CharacterFocusController(CharacterVisual visual)
    {
        _visual = visual;
    }

    public void OnPointerEnter(bool isDialogueRunning)
    {
        IsMouseOver = true;
        if (!IsFocusLocked && !isDialogueRunning)
        {
            _visual.SetFocus(true);
        }
    }

    public void OnPointerExit()
    {
        IsMouseOver = false;
        if (!IsFocusLocked)
        {
            _visual.SetFocus(false);
        }
    }

    public void LockFocus()
    {
        IsFocusLocked = true;
        _visual.SetFocus(true);
    }

    public void ReleaseFocusLock()
    {
        IsFocusLocked = false;
        if (!IsMouseOver)
        {
            _visual.SetFocus(false);
        }
    }
}

public sealed class CharacterExecutionClickHandler : IExecutionClickHandler
{
    public bool TryHandleClick(
        CharacterAI ai,
        CharacterData characterData,
        DialogueRunner dialogueRunner,
        CharacterVisual visual,
        CharacterExecutionState state)
    {
        ExecutionManager executionManager = ExecutionManager.Instance;
        if (executionManager == null || !executionManager.IsAiming) return false;
        if (ai == null) return false;

        if (ai.MyRole == Role.Roles.겁쟁이 &&
            !state.HasShownExecutionDialogue &&
            characterData != null &&
            dialogueRunner != null)
        {
            string executionNode = characterData.dialogueNodeName + "_Coward_Execution";
            if (dialogueRunner.Dialogue.NodeExists(executionNode))
            {
                state.HasShownExecutionDialogue = true;
                executionManager.SetPendingTarget(ai);
                visual.SetFocus(true);
                dialogueRunner.StartDialogue(executionNode);
                executionManager.ToggleAiming(false);
                return true;
            }
        }

        executionManager.ExecuteTarget(ai);
        return true;
    }
}

public sealed class CharacterDialogueVariableBinder : ICharacterDialogueVariableBinder
{
    public void BindDefaultTargetName(DialogueRunner dialogueRunner, CharacterAI ai)
    {
        if (dialogueRunner == null || ai == null) return;

        CharacterAI effectiveTarget = ai.CurrentLieTarget;
        if (effectiveTarget == null && ai.LastAction != null)
        {
            effectiveTarget = ai.LastAction.TargetAI;
        }

        string targetName = effectiveTarget != null ? effectiveTarget.DisplayName : "누군가";
        SetTargetName(dialogueRunner, targetName);
    }

    public void SetTargetName(DialogueRunner dialogueRunner, string targetName)
    {
        if (dialogueRunner == null) return;
        dialogueRunner.VariableStorage.SetValue("$targetName", string.IsNullOrEmpty(targetName) ? "누군가" : targetName);
    }
}

public interface IRoleDialogueSuffixResolver
{
    string ResolveSuffix(CharacterAI ai, AIAction action);
}

public sealed class BelieverDialogueSuffixResolver : IRoleDialogueSuffixResolver
{
    public string ResolveSuffix(CharacterAI ai, AIAction action)
    {
        if (action.ActionType == AIActionType.BelieverStayHome ||
            (action.ActionType == AIActionType.BelieverInvestigate && action.Target == null))
        {
            return string.Empty;
        }

        if (action.ActionType == AIActionType.BelieverBodyFound || action.ActionType == AIActionType.WitchAttack)
        {
            return "_Believer_BodyFound";
        }

        if (action.ActionType == AIActionType.BelieverAbsent)
        {
            return "_Believer_Absent";
        }

        return action.Success ? "_Believer_Success" : "_Believer_Refused";
    }
}

public sealed class InsomniacDialogueSuffixResolver : IRoleDialogueSuffixResolver
{
    public string ResolveSuffix(CharacterAI ai, AIAction action)
    {
        if (action.ActionType == AIActionType.InsomniacWalk || (action.PretendRole.HasValue && action.Success))
        {
            return "_Insomniac_Out";
        }

        return "_Insomniac_Home";
    }
}

public sealed class CowardDialogueSuffixResolver : IRoleDialogueSuffixResolver
{
    public string ResolveSuffix(CharacterAI ai, AIAction action)
    {
        return action.ActionType == AIActionType.CowardPlea ? "_Coward_Plea" : string.Empty;
    }
}

public sealed class MuteDialogueSuffixResolver : IRoleDialogueSuffixResolver
{
    public string ResolveSuffix(CharacterAI ai, AIAction action)
    {
        return "_Mute_Silent";
    }
}

public sealed class CitizenDialogueSuffixResolver : IRoleDialogueSuffixResolver
{
    public string ResolveSuffix(CharacterAI ai, AIAction action)
    {
        if (ai != null && ai.TryGetReceivedPrayerForCitizenDialogue(out bool receivedPrayer) && receivedPrayer)
        {
            return "_Received_Prayer";
        }

        return "_No_Prayer";
    }
}

public sealed class CharacterDialogueNodeResolver : ICharacterDialogueNodeResolver
{
    private readonly Dictionary<Role.Roles, IRoleDialogueSuffixResolver> _roleResolvers;

    public CharacterDialogueNodeResolver()
    {
        _roleResolvers = new Dictionary<Role.Roles, IRoleDialogueSuffixResolver>
        {
            { Role.Roles.신자, new BelieverDialogueSuffixResolver() },
            { Role.Roles.불면증, new InsomniacDialogueSuffixResolver() },
            { Role.Roles.겁쟁이, new CowardDialogueSuffixResolver() },
            { Role.Roles.벙어리, new MuteDialogueSuffixResolver() },
            { Role.Roles.시민, new CitizenDialogueSuffixResolver() }
        };
    }

    public DialogueNodeResolution Resolve(CharacterAI ai, string baseNode, AIContext context)
    {
        if (ai == null || ai.LastAction == null)
        {
            return new DialogueNodeResolution(baseNode, null);
        }

        AIAction action = ai.LastAction;
        if (action.ActionType == AIActionType.MuteSilent)
        {
            return new DialogueNodeResolution(baseNode + "_Mute_Silent", null);
        }

        Role.Roles activeRole = action.PretendRole ?? ai.MyRole;
        string suffix = string.Empty;

        if (_roleResolvers.TryGetValue(activeRole, out IRoleDialogueSuffixResolver roleResolver))
        {
            suffix = roleResolver.ResolveSuffix(ai, action);
        }

        string targetNameOverride = null;
        bool wasHome = string.IsNullOrEmpty(suffix) || suffix == "_Insomniac_Home" || suffix == "_Received_Prayer";
        if (!ai.ShouldIgnorePrayerDialogueOverride && wasHome && context != null && context.HasReceivedPrayer(ai))
        {
            suffix = "_Received_Prayer";
            if (context.TryGetSuccessfulBelieverVisitorName(ai, out string visitorName))
            {
                targetNameOverride = visitorName;
            }
        }

        return new DialogueNodeResolution(baseNode + suffix, targetNameOverride);
    }
}
