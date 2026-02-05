using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    [Header("Character Objects")]
    [SerializeField] private List<GameObject> characterObjects = new List<GameObject>();

    [Header("Level Info")]
    [SerializeField] private List<LevelData> levels = new List<LevelData>();
    private int currentLevelIndex = 0;

    [Header("Round Info")]
    [SerializeField] private List<Role.Roles> activeRoles = new List<Role.Roles>();
    [SerializeField] private int nightIndex = 1;
    [SerializeField] private bool hasEmptyHouseForThief;

    private bool isNight = false;

    private List<CharacterAI> participants = new List<CharacterAI>();
    private List<CharacterAI> deadParticipants = new List<CharacterAI>();
    private AIContext currentContext;
    private RoleAssigner roleAssigner = new RoleAssigner();

    public AIContext CurrentContext => currentContext;
    public bool IsNight => isNight;

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

        InitializeGame();
    }

    private IEnumerator StartFirstNight()
    {
        yield return null;

        if (IntroManager.Instance != null)
        {
            yield return new WaitWhile(() => IntroManager.Instance.IsIntroPlaying);
        }

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

    private IEnumerator NightSequence()
    {
        isNight = true;

        // 패배 조건 확인
        if (CheckDefeatCondition())
        {
            isNight = false;
            yield break;
        }

        Debug.Log("--- 밤이 시작되었습니다 ---");
        
        // 밤 로직 실행
        RunNight();

        // 밤 연출 대기
        yield return new WaitForSeconds(3.0f);

        // 아침 로직 실행
        RunMorning();

        Debug.Log("--- 아침이 밝았습니다 ---");
        
        // 승리 조건 확인
        if (CheckWinCondition())
        {
             isNight = false;
             yield break;
        }

        isNight = false;

        if (Timer.Instance != null)
        {
            Timer.Instance.ResetTimer();
        }
    }

    private bool CheckDefeatCondition()
    {
        int deadCivilians = 0;
        foreach(CharacterAI dead in deadParticipants)
        {
            if(dead.MyRole != Role.Roles.마녀) 
            {
                deadCivilians++;
            }
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
                return true;
            }
        }
        return false;
    }

    public bool CheckWinCondition()
    {
        participants.RemoveAll(ai => ai == null || !ai.gameObject.activeSelf);

        bool witchAlive = participants.Exists(ai => ai.MyRole == Role.Roles.마녀);
        if (!witchAlive)
        {
            Debug.Log("<color=green>모든 마녀가 제거되었습니다. 승리!</color>");
            if (Timer.Instance != null) Timer.Instance.StopTimer();
            
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
            return;
        }

        LevelData data = levels[levelIndex];
        Debug.Log($"--- Level {levelIndex + 1}: {data.LevelName} 시작 ---");

        List<CharacterData> shuffledData = new List<CharacterData>(data.CharacterDatas);
        ShuffleList(shuffledData);

        for (int i = 0; i < characterObjects.Count; i++)
        {
            if (i >= shuffledData.Count) 
            {
                Debug.LogWarning($"레벨 데이터의 캐릭터 수가 부족합니다. (필요: {characterObjects.Count}, 보유: {shuffledData.Count})");
                break;
            }

            CharacterInteraction interaction = characterObjects[i].GetComponent<CharacterInteraction>();
            if (interaction != null)
            {
                interaction.SetCharacterData(shuffledData[i]);
            }
        }

        nightIndex = 1;
        isNight = false;
        
        AssignRandomRoles();
        StartCoroutine(StartFirstNight());
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void OnCharacterExecuted(CharacterAI victim)
    {
        if (participants.Contains(victim))
        {
            participants.Remove(victim);
            deadParticipants.Add(victim);
            Debug.Log($"[AIManager] {victim.name}가 처형되어 참가자 명단에서 제외되었습니다.");
        }

        victim.gameObject.SetActive(false);

        CheckWinCondition();
    }

    public void AssignRandomRoles()
    {
        roleAssigner.AssignRoles(characterObjects, activeRoles);
        
        // 참가자 리스트 재구축
        participants.Clear();
        deadParticipants.Clear();
        foreach (GameObject obj in characterObjects)
        {
            CharacterAI ai = obj.GetComponent<CharacterAI>();
            if (ai != null)
            {
                participants.Add(ai);
            }
        }

        Debug.Log("<color=green>모든 캐릭터에게 새로운 직업이 부여되었습니다.</color>");
    }

    public void RunNight()
    {
        bool isEvenNight = nightIndex % 2 == 0;
        bool believerInvestigating = participants.Exists(ai => ai is BelieverAI);
        bool insomniacWalking = isEvenNight && participants.Exists(ai => ai is InsomniacAI);
        hasEmptyHouseForThief = insomniacWalking || believerInvestigating;

        BuildContext();

        foreach (CharacterAI ai in participants)
        {
            if (ai == null) continue;

            if (ai is BelieverAI)
                currentContext.OutOfHouse.Add(ai);

            if (ai is InsomniacAI && (nightIndex % 2 == 0))
                currentContext.OutOfHouse.Add(ai);
        }

        foreach (CharacterAI ai in participants)
        {
            if (ai != null) ai.DoNightAction(currentContext);
        }
    }

    public void RunMorning()
    {
        if (currentContext == null)
        {
            BuildContext();
        }

        // 1. 모든 참가자들의 행동 해결 (사망자 포함, 밤 동안의 행동 결과를 확정)
        // 신자들의 행동 먼저 해결 (다른 역할들이 참고할 수 있는 상태를 설정함, 예: PrayerReceived)
        foreach (CharacterAI ai in participants)
        {
            if (ai != null && ai is BelieverAI) ai.ResolveMorning(currentContext);
        }

        foreach (CharacterAI ai in participants)
        {
            if (ai != null && !(ai is BelieverAI)) ai.ResolveMorning(currentContext);
        }

        // 2. 밤 사이 공격받은 희생자 처리
        foreach (CharacterAI victim in currentContext.Attacked)
        {
            if (victim != null)
            {
                Debug.Log($"{victim.name} was attacked during the night.");
                victim.gameObject.SetActive(false);

                participants.Remove(victim);
                deadParticipants.Add(victim);
            }
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
