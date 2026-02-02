using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

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

        // 게임 시작 시 자동으로 역할 할당
        AssignRandomRoles();

        // 인트로가 끝난 후 첫 번째 밤을 시작하도록 함
        StartCoroutine(StartFirstNight());
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

    private IEnumerator NightSequence()
    {
        // 0. 패배 조건 체크 (은탄 부족 & 마녀 생존)
        if (ExecutionManager.Instance != null && ExecutionManager.Instance.CurrentBullets <= 0)
        {
            bool witchAlive = participants.Exists(ai => ai.MyRole == Role.Roles.마녀);
            if (witchAlive)
            {
                Debug.Log("<color=red>은탄이 다 떨어졌습니다. 마녀에게 습격당해 게임 오버!</color>");
                // TODO: 게임 오버 연출 (Game Over Scene or UI)
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
            yield break; // 승리했으면 타이머 재개 안 함
        }

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
            // TODO: 승리 UI, 다음 레벨 로드
            if (Timer.Instance != null) Timer.Instance.StopTimer();
            return true;
        }
        return false;
    }

    public void OnCharacterExecuted(CharacterAI victim)
    {
        if (participants.Contains(victim))
        {
            participants.Remove(victim);
            Debug.Log($"[AIManager] {victim.name}가 처형되어 참가자 명단에서 제외되었습니다.");
        }

        // 실제 오브젝트 비활성화
        victim.gameObject.SetActive(false);

        // 승리 체크
        CheckWinCondition();
    }

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

        // 2. 리스트 섞기
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
