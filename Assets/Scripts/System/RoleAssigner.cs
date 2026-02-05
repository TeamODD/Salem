using System.Collections.Generic;
using UnityEngine;

public class RoleAssigner
{
    public void AssignRoles(List<GameObject> characterObjects, List<Role.Roles> activeRoles)
    {
        if (characterObjects.Count != 5)
        {
            Debug.LogWarning("캐릭터 오브젝트 개수가 5개가 아닙니다. 로직이 의도와 다르게 동작할 수 있습니다.");
        }

        // 기존 AI 컴포넌트 제거 및 오브젝트 활성화
        foreach (GameObject obj in characterObjects)
        {
            CharacterAI oldAI = obj.GetComponent<CharacterAI>();
            if (oldAI != null)
            {
                Object.Destroy(oldAI);
            }
            obj.SetActive(true);
        }

        activeRoles.Clear();

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

        // 캐릭터 수에 맞춰 역할 리스트 조정 (부족하면 시민 추가, 많으면 제거)
        while (activeRoles.Count < characterObjects.Count)
        {
            activeRoles.Add(Role.Roles.시민);
        }
        while (activeRoles.Count > characterObjects.Count)
        {
            activeRoles.RemoveAt(activeRoles.Count - 1);
        }

        // 최종 역할 리스트 섞기
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
                newAI.SetDisplayName($"{i + 1}");
            }
        }
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
}
