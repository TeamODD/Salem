using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameManager
{
    public IEnumerator LoadLevelRoutine(int levelIndex)
    {
        if (levelIndex >= levels.Count)
        {
            Debug.Log("<color=green>모든 레벨을 클리어했습니다! 게임 종료.</color>");
            ChangeState(new GameCompleteState(this));
            yield break;
        }

        currentLevelIndex = levelIndex;
        LevelData data = levels[levelIndex];
        Debug.Log($"--- Level {levelIndex + 1}: {data.LevelName} 로드 시작 ---");

        if (data.RolesInLevel != null && data.RolesInLevel.Count > 0)
        {
            activeRoles = new List<Role.Roles>(data.RolesInLevel);
        }
        else
        {
            activeRoles = new List<Role.Roles>();
        }

        ApplyCharacterData(data.CharacterDatas);

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
            IntroManager.Instance.ShowIntro(data.LevelName, activeRoles);
            yield return new WaitWhile(() => IntroManager.Instance.IsIntroPlaying);
        }

        yield return FadeInRoutine(fadeDuration);

        Debug.Log($"--- Level {levelIndex + 1} 준비 완료 (참가자: {participants.Count}명) ---");
        ChangeState(new DayState(this));
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
