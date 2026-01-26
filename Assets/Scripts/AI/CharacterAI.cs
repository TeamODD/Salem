using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterAI : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string displayName;

    [Header("Role")]
    [SerializeField] protected Role.Roles role;

    [Header("Dialogue")]
    [SerializeField] protected DialogueLibrary dialogueLibrary;

    protected AIAction lastAction;
    protected readonly List<string> nightDialogues = new List<string>();

    public Role.Roles MyRole => role;
    public AIAction LastAction => lastAction;
    public IReadOnlyList<string> NightDialogues => nightDialogues;

    public abstract void DoNightAction(AIContext context);
    public abstract void RecordDialogue(AIContext context);
    public abstract void ResolveMorning(AIContext context);

    protected void SetAction(AIContext context, string actionId, Character target = null, Role.Roles? pretendRole = null, bool success = true)
    {
        lastAction = new AIAction(actionId, target, pretendRole, success);
        if (context != null)
        {
            context.RegisterAction(this, lastAction);
        }
    }

    protected void AddDialogue(string actionId)
    {
        if (dialogueLibrary == null) return;

        string line = dialogueLibrary.GetRandomLine(role, actionId);
        if (string.IsNullOrEmpty(line)) return;

        // 2. 표시용 이름 결정 (displayName이 비어있으면 gameObject.name 사용)
        string myName = string.IsNullOrEmpty(displayName) ? gameObject.name : displayName;

        // 3. {Name} 치환
        line = line.Replace("{Name}", myName);

        // 4. lastAction 데이터 기반 치환
        if (lastAction != null)
        {
            // {Target} 치환
            if (lastAction.Target != null)
            {
                // 대상 캐릭터에게도 displayName이 있을 수 있으므로 체크
                var targetAI = lastAction.Target.GetComponent<CharacterAI>();
                string targetName = (targetAI != null && !string.IsNullOrEmpty(targetAI.displayName)) 
                    ? targetAI.displayName 
                    : lastAction.Target.name;
                
                line = line.Replace("{Target}", targetName);
            }
            else
            {
                line = line.Replace("{Target}", "누군가");
            }

            // {PretendRole} 치환
            if (lastAction.PretendRole.HasValue)
            {
                line = line.Replace("{PretendRole}", lastAction.PretendRole.Value.ToString());
            }
        }

        // 5. 최종 결과 저장
        nightDialogues.Add(line);
    }

    public void ClearNightDialogues()
    {
        nightDialogues.Clear();
    }

    public void SetRole(Role.Roles newRole)
    {
        role = newRole;
        Debug.Log($"{gameObject.name}의 역할이 {newRole}(으)로 설정되었습니다.");
    }

    public void Initialize(Role.Roles assignedRole, DialogueLibrary library)
    {
        role = assignedRole;
        dialogueLibrary = library;
        Debug.Log($"{gameObject.name}가 {assignedRole}로 초기화되었습니다.");
    }
}
