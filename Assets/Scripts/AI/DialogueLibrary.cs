using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class DialogueEntry
{
    [ActionId]
    public string actionId;
    [TextArea(2, 4)]
    public List<string> lines = new List<string>();
}

[Serializable]
public class RoleDialogueSet
{
    public Role.Roles role;
    public List<DialogueEntry> entries = new List<DialogueEntry>();
}

[CreateAssetMenu(fileName = "DialogueLibrary", menuName = "Scriptable Objects/AI/DialogueLibrary")]
public class DialogueLibrary : ScriptableObject
{
    public List<RoleDialogueSet> roleDialogueSets = new List<RoleDialogueSet>();

    public string GetRandomLine(Role.Roles role, string actionId)
    {
        var set = roleDialogueSets.Find(s => s.role == role);
        if (set == null) return null;

        var entry = set.entries.Find(e => e.actionId == actionId);
        if (entry == null || entry.lines == null || entry.lines.Count == 0) return null;

        int index = UnityEngine.Random.Range(0, entry.lines.Count);
        return entry.lines[index];
    }
}
