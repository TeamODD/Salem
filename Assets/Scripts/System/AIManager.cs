using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    [Header("Character Objects (프리펩))")]
    [SerializeField] private List<GameObject> characterObjects = new List<GameObject>();

    [Header("Round Info")]
    [SerializeField] private List<Role.Roles> activeRoles = new List<Role.Roles>();
    [SerializeField] private int nightIndex = 1;
    [SerializeField] private bool hasEmptyHouseForThief;

    private List<CharacterAI> participants = new List<CharacterAI>();
    [SerializeField] private DialogueLibrary dialogueLibrary;
    private AIContext currentContext;
    public AIContext CurrentContext => currentContext;

    public void AssignRandomRoles()
    {
        if (activeRoles.Count != characterObjects.Count)
        {
            Debug.LogError("직업 개수와 캐릭터 오브젝트 개수가 일치하지 않습니다!");
            return;
        }

        // 1. 기존에 붙어있던 AI 컴포넌트 제거 및 리스트 초기화
        foreach (var obj in characterObjects)
        {
            var oldAI = obj.GetComponent<CharacterAI>();
            if (oldAI != null) Destroy(oldAI);
            obj.SetActive(true); // 죽었던 캐릭터도 다시 활성화
        }
        participants.Clear();

        // 1. 활성화된 직업 리스트 복사
        List<Role.Roles> shuffledRoles = new List<Role.Roles>(activeRoles);

        // 2. 피셔-예이츠 셔플(Fisher-Yates Shuffle) 알고리즘으로 리스트 섞기
        for (int i = shuffledRoles.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Role.Roles temp = shuffledRoles[i];
            shuffledRoles[i] = shuffledRoles[randomIndex];
            shuffledRoles[randomIndex] = temp;
        }

        // 3. 각 오브젝트에 랜덤 직업 컴포넌트 추가
        for (int i = 0; i < characterObjects.Count; i++)
        {
            Role.Roles assignedRole = shuffledRoles[i];
            CharacterAI newAI = AddRoleComponent(characterObjects[i], assignedRole);

            if (newAI != null)
            {
                // CharacterAI에 구현된 Initialize를 통해 데이터 주입
                newAI.Initialize(assignedRole, dialogueLibrary);
                participants.Add(newAI);
            }
        }
        
        Debug.Log("<color=green>모든 캐릭터에게 새로운 직업이 부여되었습니다.</color>");
    }

    private CharacterAI AddRoleComponent(GameObject target, Role.Roles role)
    {
        switch (role)
        {
            case Role.Roles.마녀: return target.AddComponent<WitchAI>();
            case Role.Roles.신자: return target.AddComponent<BelieverAI>();
            case Role.Roles.좀도둑: return target.AddComponent<ThiefAI>();
            case Role.Roles.불면증: return target.AddComponent<InsomniacAI>();
            case Role.Roles.겁쟁이: return target.AddComponent<CowardAI>();
            case Role.Roles.벙어리: return target.AddComponent<MuteAI>();
            default: return null;
        }
    }

    public void RunNight()
    {
        foreach (var ai in participants)
        {
            if (ai != null) ai.ClearNightDialogues();
        }

        bool isEvenNight = nightIndex % 2 == 0;
        bool believerInvestigating = participants.Exists(ai => ai is BelieverAI);
        bool insomniacWalking = isEvenNight && participants.Exists(ai => ai is InsomniacAI);
        hasEmptyHouseForThief = insomniacWalking || believerInvestigating;  
    
        BuildContext();

        foreach (var ai in participants)
        {
            if (ai == null) continue;

            // 1. 신자는 무조건 조사를 위해 집을 비움
            if (ai is BelieverAI)
                currentContext.OutOfHouse.Add(ai);

            // 2. 불면증 환자는 짝수날 밤에 산책을 나감
            if (ai is InsomniacAI && (nightIndex % 2 == 0))
                currentContext.OutOfHouse.Add(ai);
        }   

        foreach (var ai in participants)
        {
            if (ai != null) ai.DoNightAction(currentContext);
        }

        foreach (var ai in participants)
        {
            if (ai != null) ai.RecordDialogue(currentContext);
        }
    }

    public void RunMorning()
    {
        if (currentContext == null)
        {
            BuildContext();
        }

        // 공격당한 캐릭터 처리
        foreach (var victim in currentContext.Attacked)
        {
            if (victim != null)
            {
                Debug.Log($"{victim.name} was attacked during the night.");
                victim.gameObject.SetActive(false);

                participants.Remove(victim);
            }
        }

        // 생존자 정산
        foreach (var ai in participants)
        {
            if (ai != null) ai.ResolveMorning(currentContext);
        }

        nightIndex += 1;
    }

    private void BuildContext()
    {
        currentContext = new AIContext
        {
            NightIndex = nightIndex,
            HasEmptyHouseForThief = hasEmptyHouseForThief
        };

        currentContext.Participants.AddRange(participants);
        currentContext.ActiveRoles.AddRange(activeRoles);
    }
}
