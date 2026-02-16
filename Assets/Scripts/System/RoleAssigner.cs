using System.Collections.Generic;
using UnityEngine;

public class RoleAssigner
{
    public List<CharacterAI> AssignRoles(List<GameObject> characterObjects, List<Role.Roles> activeRoles)
    {
        List<CharacterAI> newParticipants = new List<CharacterAI>();

        // 만약 activeRoles가 비어있다면 (랜덤 레벨인 경우) 기존의 랜덤 생성 로직 수행
        if (activeRoles == null || activeRoles.Count == 0)
        {
            activeRoles = new List<Role.Roles>();
            // 필수 역할 추가
            activeRoles.Add(Role.Roles.마녀);
            activeRoles.Add(Role.Roles.신자);

            // 남은 특성 목록
            List<Role.Roles> remainingTraits = new List<Role.Roles>
            {
                Role.Roles.좀도둑,
                Role.Roles.불면증,
                Role.Roles.겁쟁이,
                Role.Roles.벙어리
            };

            // 특성 목록 섞기
            for (int i = remainingTraits.Count - 1; i > 0; i--)
            {
                int rnd = Random.Range(0, i + 1);
                Role.Roles temp = remainingTraits[i];
                remainingTraits[i] = remainingTraits[rnd];
                remainingTraits[rnd] = temp;
            }

            // 시민 포함 여부 결정 (50% 확률)
            bool includeCitizen = Random.value < 0.5f;
            int traitsCount = includeCitizen ? 2 : 3;

            for (int i = 0; i < traitsCount; i++)
            {
                if (i < remainingTraits.Count)
                {
                    activeRoles.Add(remainingTraits[i]);
                }
            }

            if (includeCitizen)
            {
                activeRoles.Add(Role.Roles.시민);
            }
        }

        // 기존 AI 컴포넌트 제거 및 오브젝트 활성화
        foreach (GameObject obj in characterObjects)
        {
            CharacterAI oldAI = obj.GetComponent<CharacterAI>();
            if (oldAI != null)
            {
                Object.DestroyImmediate(oldAI); // 즉시 제거하여 새 컴포넌트와 혼동 방지
            }
            obj.SetActive(true);
        }

        // 캐릭터 수에 맞춰 역할 리스트 조정 (부족하면 시민 추가, 많으면 제거)
        while (activeRoles.Count < characterObjects.Count)
        {
            activeRoles.Add(Role.Roles.시민);
        }
        while (activeRoles.Count > characterObjects.Count)
        {
            activeRoles.RemoveAt(activeRoles.Count - 1);
        }

        // 최종 역할 리스트 섞기 (여기서 activeRoles 자체를 섞으면 호출한 쪽의 리스트 순서가 바뀜)
        List<Role.Roles> shuffledRoles = new List<Role.Roles>(activeRoles);
        for (int i = shuffledRoles.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Role.Roles temp = shuffledRoles[i];
            shuffledRoles[i] = shuffledRoles[randomIndex];
            shuffledRoles[randomIndex] = temp;
        }

        // 역할 및 컴포넌트 할당
        for (int i = 0; i < characterObjects.Count; i++)
        {
            // Character 컴포넌트가 없으면 추가
            if (characterObjects[i].GetComponent<Character>() == null)
            {
                characterObjects[i].AddComponent<Character>();
            }

            Role.Roles assignedRole = shuffledRoles[i];
            CharacterAI newAI = AddRoleComponent(characterObjects[i], assignedRole);

            if (newAI != null)
            {
                newAI.Initialize(assignedRole);
                newAI.SetDisplayName(GetCharacterDisplayName(characterObjects[i], i));
                newParticipants.Add(newAI);
            }
        }

        return newParticipants;
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
            case Role.Roles.시민: return target.AddComponent<CitizenAI>();
            default: return null;
        }
    }

    private string GetCharacterDisplayName(GameObject characterObject, int index)
    {
        if (characterObject == null)
        {
            return $"{index + 1}";
        }

        CharacterInteraction interaction = characterObject.GetComponent<CharacterInteraction>();
        if (interaction != null && !string.IsNullOrWhiteSpace(interaction.CharacterName))
        {
            return interaction.CharacterName;
        }

        return $"{index + 1}";
    }
}
