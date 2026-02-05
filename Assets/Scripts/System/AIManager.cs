using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    [Header("Character Objects (프리펩))")]
    [SerializeField] private List<GameObject> characterObjects = new List<GameObject>();

    [Header("Level Info")]
    [SerializeField] private List<LevelData> levels = new List<LevelData>();
    private int currentLevelIndex = 0;

    [Header("Round Info")]
    [SerializeField] private List<Role.Roles> activeRoles = new List<Role.Roles>();
    [SerializeField] private int nightIndex = 1;
    [SerializeField] private bool hasEmptyHouseForThief;

    private bool isNight = false; // 현재 밤인지 여부

    private List<CharacterAI> participants = new List<CharacterAI>();
    private List<CharacterAI> deadParticipants = new List<CharacterAI>();
    private AIContext currentContext;
    public AIContext CurrentContext => currentContext;
    public bool IsNight => isNight; // 외부에서 확인용

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (Timer.Instance != null)
        {
            Timer.Instance.OnTimeUp += OnTimerEnded;
        }

        // 게임 초기화 및 첫 번째 레벨 로드
        InitializeGame();
    }

    private IEnumerator StartFirstNight()
    {
        // 다른 스크립트들의 Start가 실행될 시간을 주기 위해 1프레임 대기
        yield return null;

        // 인트로가 진행 중이라면 끝날 때까지 대기
        if (IntroManager.Instance != null)
        {
            yield return new WaitWhile(() => IntroManager.Instance.IsIntroPlaying);
        }

        // 바로 밤 시퀀스 시작
        StartCoroutine(NightSequence());
    }

    private void OnDestroy()
    {
        if (Timer.Instance != null)
        {
            Timer.Instance.OnTimeUp -= OnTimerEnded;
        }
    }

    private void OnTimerEnded()
    {
        StartCoroutine(NightSequence());
    }

    // 낮에 스킵 버튼을 눌렀을 때 호출
    public void SkipDay()
    {
        if (isNight) 
        {
            Debug.Log("지금은 밤이라 스킵할 수 없습니다.");
            return; 
        }

        if (Timer.Instance != null)
        {
            Debug.Log("낮 시간을 건너뜁니다.");
            Timer.Instance.FinishImmediately();
        }
    }

        private System.Collections.IEnumerator NightSequence()
        {
            isNight = true;
    
            // 0. 패배 조건 체크 (은탄 부족 OR 시민 3명 이상 사망 & 마녀 생존)
            int deadCivilians = 0;
            foreach(var dead in deadParticipants)
            {
                if(dead.MyRole != Role.Roles.마녀) deadCivilians++;
            }

            bool isBulletEmpty = (ExecutionManager.Instance != null && ExecutionManager.Instance.CurrentBullets <= 0);
            bool isTooManyDead = (deadCivilians >= 3);

            if (isBulletEmpty || isTooManyDead)
            {
                bool witchAlive = participants.Exists(ai => ai.MyRole == Role.Roles.마녀);
                if (witchAlive)
                {
                    string reason = isBulletEmpty ? "은탄이 다 떨어졌습니다." : "시민이 너무 많이 희생되었습니다.";
                    Debug.Log($"<color=red>{reason} 마녀에게 습격당해 게임 오버!</color>");
                    // TODO: 게임 오버 연출 (Game Over Scene or UI)
                    isNight = false;
                    yield break; // 밤 로직 진행하지 않고 종료
                }
            }
    
            Debug.Log("--- 밤이 시작되었습니다 ---");
            
            // 1. 페이드 아웃 (UI 연결 필요)
            // yield return GlobalFadeManager.Instance.FadeOut(); 
            
            // 2. 밤 로직 실행
            RunNight();
    
            // 3. 밤 연출 대기 (3초)
            yield return new WaitForSeconds(3.0f);
    
            // 4. 아침 로직 실행 (결과 적용)
            RunMorning();
    
            // 5. 페이드 인
            // yield return GlobalFadeManager.Instance.FadeIn();
    
            Debug.Log("--- 아침이 밝았습니다 ---");
            
            // 6. 승리 조건 체크
            if (CheckWinCondition())
            {
                 isNight = false;
                 yield break; // 승리했으면 타이머 재개 안 함
            }
    
            isNight = false;
    
            // 7. 다음 낮 타이머 시작
            if (Timer.Instance != null)
            {
                Timer.Instance.ResetTimer();
            }
        }
    public bool CheckWinCondition()
    {
        // 리스트에서 비활성화된 객체나 null 제거 후 확인하는 게 안전함
        participants.RemoveAll(ai => ai == null || !ai.gameObject.activeSelf);

        bool witchAlive = participants.Exists(ai => ai.MyRole == Role.Roles.마녀);
        if (!witchAlive)
        {
            Debug.Log("<color=green>모든 마녀가 제거되었습니다. 승리!</color>");
            if (Timer.Instance != null) Timer.Instance.StopTimer();
            
            // 다음 레벨 로드
            currentLevelIndex++;
            LoadLevel(currentLevelIndex);
            
            return true;
        }
        return false;
    }

    private void InitializeGame()
    {
        currentLevelIndex = 0;
        LoadLevel(currentLevelIndex);
    }
    
    private void LoadLevel(int levelIndex)
    {
        if (levelIndex >= levels.Count)
        {
            Debug.Log("<color=green>모든 레벨을 클리어했습니다! 게임 종료.</color>");
            // TODO: 엔딩 크레딧 등으로 이동
            return;
        }

        LevelData data = levels[levelIndex];
        Debug.Log($"--- Level {levelIndex + 1}: {data.LevelName} 시작 ---");

        // 1. 캐릭터 데이터 풀(Pool) 복사 및 셔플
        List<CharacterData> shuffledData = new List<CharacterData>(data.CharacterDatas);
        for (int i = shuffledData.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CharacterData temp = shuffledData[i];
            shuffledData[i] = shuffledData[randomIndex];
            shuffledData[randomIndex] = temp;
        }

        // 2. 캐릭터 오브젝트에 데이터 할당
        for (int i = 0; i < characterObjects.Count; i++)
        {
            // 데이터가 모자라면 중단 (혹은 반복 할당할 수도 있으나, 여기선 중단)
            if (i >= shuffledData.Count) 
            {
                Debug.LogWarning($"레벨 데이터의 캐릭터 수가 부족합니다. (필요: {characterObjects.Count}, 보유: {shuffledData.Count})");
                break;
            }

            var interaction = characterObjects[i].GetComponent<CharacterInteraction>();
            if (interaction != null)
            {
                interaction.SetCharacterData(shuffledData[i]);
            }
        }

        // 3. 게임 상태 초기화
        nightIndex = 1;
        isNight = false;
        
        // 은탄 충전 (필요하다면 ExecutionManager에 리셋 함수 추가 필요)
        // if (ExecutionManager.Instance != null) ExecutionManager.Instance.ResetBullets(); 

        // 4. 역할 재분배 및 밤 시작
        AssignRandomRoles();
        StartCoroutine(StartFirstNight());
    }

    public void OnCharacterExecuted(CharacterAI victim)
    {
        if (participants.Contains(victim))
        {
            participants.Remove(victim);
            deadParticipants.Add(victim);
            Debug.Log($"[AIManager] {victim.name}가 처형되어 참가자 명단에서 제외되었습니다.");
        }

        // 실제 오브젝트 비활성화
        victim.gameObject.SetActive(false);

        // 승리 체크
        CheckWinCondition();
    }

    public void AssignRandomRoles()
    {
        if (characterObjects.Count != 5)
        {
            Debug.LogWarning("캐릭터 오브젝트 개수가 5개가 아닙니다. 로직이 의도와 다르게 동작할 수 있습니다.");
        }

        // 1. 기존 AI 제거 및 초기화
        foreach (var obj in characterObjects)
        {
            var oldAI = obj.GetComponent<CharacterAI>();
            if (oldAI != null) Destroy(oldAI);
            obj.SetActive(true);
        }
        participants.Clear();
        deadParticipants.Clear();
        activeRoles.Clear(); // activeRoles 재설정

        // 2. 역할 풀 생성 (규칙 1 적용)
        // 필수: 마녀 1, 신자 1
        activeRoles.Add(Role.Roles.마녀);
        activeRoles.Add(Role.Roles.신자);

        // 남은 특성: 좀도둑, 불면증, 겁쟁이, 벙어리
        List<Role.Roles> remainingTraits = new List<Role.Roles> 
        { 
            Role.Roles.좀도둑, 
            Role.Roles.불면증, 
            Role.Roles.겁쟁이, 
            Role.Roles.벙어리 
        };

        // 랜덤 섞기
        for (int i = remainingTraits.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            var temp = remainingTraits[i];
            remainingTraits[i] = remainingTraits[rnd];
            remainingTraits[rnd] = temp;
        }

        // 시민 포함 여부 결정 (0~1명)
        // 남은 자리는 3개 (총 5명 기준)
        // 경우의 수:
        // A. 시민 0명 -> 남은 특성 3개
        // B. 시민 1명 -> 남은 특성 2개

        bool includeCitizen = Random.value < 0.5f; // 50% 확률로 시민 포함
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

        // 만약 activeRoles가 캐릭터 수보다 적다면 (예외 처리), 나머지를 시민으로 채움
        while (activeRoles.Count < characterObjects.Count)
        {
            activeRoles.Add(Role.Roles.시민);
        }
        // 만약 많다면 자름
        while (activeRoles.Count > characterObjects.Count)
        {
            activeRoles.RemoveAt(activeRoles.Count - 1);
        }

        // 3. 리스트 섞기 (최종 배정)
        List<Role.Roles> shuffledRoles = new List<Role.Roles>(activeRoles);
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
            // Character 컴포넌트 확인 및 추가
            if (characterObjects[i].GetComponent<Character>() == null)
            {
                characterObjects[i].AddComponent<Character>();
            }

            Role.Roles assignedRole = shuffledRoles[i];
            CharacterAI newAI = AddRoleComponent(characterObjects[i], assignedRole);

            if (newAI != null)
            {
                newAI.Initialize(assignedRole, null);
                newAI.SetDisplayName($"{i + 1}"); 
                participants.Add(newAI);
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
                deadParticipants.Add(victim);
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
        currentContext.DeadParticipants.AddRange(deadParticipants);
        currentContext.ActiveRoles.AddRange(activeRoles);
    }
}
