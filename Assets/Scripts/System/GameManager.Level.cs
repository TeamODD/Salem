using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameManager
{
    private static readonly Role.Roles[] InfiniteModeOptionalRoles =
    {
        Role.Roles.좀도둑,
        Role.Roles.불면증,
        Role.Roles.겁쟁이,
        Role.Roles.벙어리,
        Role.Roles.시민
    };

    public IEnumerator LoadLevelRoutine(int levelIndex)
    {
        if (!infiniteMode && levelIndex >= levels.Count)
        {
            Debug.Log("<color=green>모든 레벨을 클리어했습니다! 게임 종료.</color>");
            ChangeState(new GameCompleteState(this));
            yield break;
        }

        currentLevelIndex = levelIndex;
        totalStagesPlayed = levelIndex + 1;
        string levelName;
        List<CharacterData> characterDataPool;

        if (infiniteMode)
        {
            levelName = $"Stage {levelIndex + 1}";
            activeRoles = GenerateInfiniteModeRoles();
            characterDataPool = CollectInfiniteModeCharacterDataPool();
            Debug.Log($"--- Infinite {levelName} 로드 시작 ---");
        }
        else
        {
            LevelData data = levels[levelIndex];
            levelName = data.LevelName;
            Debug.Log($"--- Level {levelIndex + 1}: {levelName} 로드 시작 ---");

            if (data.RolesInLevel != null && data.RolesInLevel.Count > 0)
            {
                activeRoles = new List<Role.Roles>(data.RolesInLevel);
            }
            else
            {
                activeRoles = new List<Role.Roles>();
            }

            characterDataPool = data.CharacterDatas;
        }

        ApplyCharacterData(characterDataPool);

        participants.Clear();
        deadParticipants.Clear();
        lastNightDeathNames.Clear();
        processedDeadCountInLevel = 0;

        Debug.Log($"[GameManager] 역할 할당 시작. 오브젝트 수: {characterObjects.Count}, 예정된 역할 수: {activeRoles.Count}");
        List<CharacterAI> newParticipants = roleAssigner.AssignRoles(characterObjects, activeRoles);
        participants.AddRange(newParticipants);
        RoleGuessManager.Instance?.ResetAllMarksToDefault();
        Debug.Log($"[GameManager] 역할 할당 완료. 생성된 참가자 수: {participants.Count}");

        nightIndex = 1;
        isNight = false;
        hasEmptyHouseForThief = false;
        ExecutionManager.Instance?.ResetState();

        Debug.Log("--- 첫 번째 밤 시뮬레이션 시작 ---");
        RunNight();
        RunMorning();
        Debug.Log("--- 첫 번째 밤 시뮬레이션 종료 (Day 1 시작) ---");

        if (IntroManager.Instance != null)
        {
            yield return new WaitForSeconds(introLeadDelay);
            IntroManager.Instance.ShowIntro(levelName, activeRoles);
            yield return new WaitWhile(() => IntroManager.Instance.IsIntroPlaying);
        }

        yield return FadeInRoutine(fadeDuration);

        Debug.Log($"--- Level {levelIndex + 1} 준비 완료 (참가자: {participants.Count}명) ---");
        ChangeState(new DayState(this));
    }

    private List<Role.Roles> GenerateInfiniteModeRoles()
    {
        int maxUniqueRoleCount = 2 + InfiniteModeOptionalRoles.Length;
        if (characterObjects.Count > maxUniqueRoleCount)
        {
            Debug.LogWarning($"[GameManager] 무한모드는 최대 {maxUniqueRoleCount}개의 고유 역할만 지원합니다. 현재 캐릭터 수: {characterObjects.Count}");
        }

        List<Role.Roles> generatedRoles = new List<Role.Roles>
        {
            Role.Roles.마녀,
            Role.Roles.신자
        };

        List<Role.Roles> optionalRoles = new List<Role.Roles>(InfiniteModeOptionalRoles);
        for (int i = optionalRoles.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Role.Roles temp = optionalRoles[i];
            optionalRoles[i] = optionalRoles[randomIndex];
            optionalRoles[randomIndex] = temp;
        }

        int targetRoleCount = Mathf.Min(characterObjects.Count, 2 + optionalRoles.Count);
        for (int i = 0; i < optionalRoles.Count && generatedRoles.Count < targetRoleCount; i++)
        {
            generatedRoles.Add(optionalRoles[i]);
        }

        return generatedRoles;
    }

    private List<CharacterData> CollectInfiniteModeCharacterDataPool()
    {
        List<CharacterData> pooledData = new List<CharacterData>();

        for (int i = 0; i < levels.Count; i++)
        {
            LevelData level = levels[i];
            if (level == null || level.CharacterDatas == null) continue;

            for (int j = 0; j < level.CharacterDatas.Count; j++)
            {
                CharacterData data = level.CharacterDatas[j];
                if (data != null && !pooledData.Contains(data))
                {
                    pooledData.Add(data);
                }
            }
        }

        return pooledData;
    }

    private void ApplyCharacterData(List<CharacterData> characterDataList)
    {
        List<CharacterData> dataToApply = characterDataList != null
            ? new List<CharacterData>(characterDataList)
            : new List<CharacterData>();
        if (dataToApply.Count == 0) return;

        // 레벨 시작 시마다 외형 목록을 섞어 직업 배정과 독립적으로 랜덤 외형을 적용한다.
        for (int i = dataToApply.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CharacterData temp = dataToApply[i];
            dataToApply[i] = dataToApply[randomIndex];
            dataToApply[randomIndex] = temp;
        }

        for (int i = 0; i < characterObjects.Count; i++)
        {
            if (i >= dataToApply.Count) break;

            CharacterInteraction interaction = characterObjects[i].GetComponent<CharacterInteraction>();
            if (interaction != null)
            {
                interaction.SetCharacterData(dataToApply[i]);
            }
        }
    }
}
