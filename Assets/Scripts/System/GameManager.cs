using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class GameManager : MonoBehaviour, IGameFlowContext
{
    private const float RoundTimeReductionPerStage = 10f;

    public static GameManager Instance { get; private set; }

    [Header("Character Objects")]
    [SerializeField] private List<GameObject> characterObjects = new List<GameObject>();

    [Header("Level Info")]
    [SerializeField] private List<LevelData> levels = new List<LevelData>();
    [SerializeField] private int currentLevelIndex = 0;
    [SerializeField] private bool infiniteMode = false;

    [Header("Round Info")]
    [SerializeField] private List<Role.Roles> activeRoles = new List<Role.Roles>();
    [SerializeField] private int nightIndex = 1;
    [SerializeField] private bool hasEmptyHouseForThief;

    [Header("Flow Timing")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float introLeadDelay = 0.5f;
    [SerializeField] private float nightResolveDelay = 2.0f;
    [SerializeField] private float nightDeathNoticeDuration = 1.8f;

    private bool isNight;
    private bool isTransitioning;

    private readonly List<CharacterAI> participants = new List<CharacterAI>();
    private readonly List<CharacterAI> deadParticipants = new List<CharacterAI>();
    private readonly List<string> lastNightDeathNames = new List<string>();
    private readonly RoleAssigner roleAssigner = new RoleAssigner();

    private AIContext currentContext;
    private IGameFlowState currentState;
    private Coroutine stateRoutine;

    private int totalStagesPlayed;
    private int totalSacrificedExcludingWitch;
    private int totalCorrectMemoCount;
    private int processedDeadCountInLevel;

    public AIContext CurrentContext => currentContext;
    public bool IsNight => isNight;
    public bool IsTransitioning => isTransitioning;

    public float FadeDuration => fadeDuration;
    public float IntroLeadDelay => introLeadDelay;
    public float NightResolveDelay => nightResolveDelay;
    public int CurrentLevelIndex => currentLevelIndex;

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
        ScoreRuntimeData.Clear();
        ResetScoreSession();

        if (Timer.Instance != null)
        {
            Timer.Instance.OnTimeUp += OnTimerEnded;
        }

        ChangeState(new BootState(this));
    }

    private void OnDestroy()
    {
        StopManagedRoutine();

        if (Timer.Instance != null)
        {
            Timer.Instance.OnTimeUp -= OnTimerEnded;
        }
    }

    private void OnTimerEnded()
    {
        currentState?.OnTimerElapsed();
    }

    public void SkipDay()
    {
        if (currentState == null || !currentState.CanSkipDay) return;
        if (Timer.Instance == null) return;

        Debug.Log("낮 시간을 건너뜁니다.");
        Timer.Instance.ExpireWithoutFill();
    }

    public void OnCharacterExecuted(CharacterAI victim)
    {
        currentState?.OnCharacterExecuted(victim);
    }

    public void ChangeState(IGameFlowState nextState)
    {
        if (nextState == null) return;

        currentState?.Exit();
        currentState = nextState;
        Debug.Log($"[GameManager] State -> {currentState.Name}");
        currentState.Enter();
    }

    public void StartManagedRoutine(IEnumerator routine)
    {
        StopManagedRoutine();
        if (routine != null)
        {
            stateRoutine = StartCoroutine(routine);
        }
    }

    public void StopManagedRoutine()
    {
        if (stateRoutine == null) return;

        StopCoroutine(stateRoutine);
        stateRoutine = null;
    }

    public void SetTransitioning(bool value)
    {
        isTransitioning = value;
    }

    public void SetNight(bool value)
    {
        isNight = value;
    }

    public void StopTimer()
    {
        Timer.Instance?.StopTimer();
    }

    public void ResetTimerPaused()
    {
        Timer.Instance?.ResetTimerPaused();
    }

    public void ResetTimer()
    {
        Timer.Instance?.ResetTimer();
    }

    public void ReduceRoundTimeForNextRound()
    {
        Timer.Instance?.AdjustGameTime(-RoundTimeReductionPerStage);
    }

    public void SetFadeOpaque()
    {
        GlobalFadeManager.Instance?.SetAlpha(1.0f);
    }

    public IEnumerator ShowDefeatUIRoutine()
    {
        if (IntroManager.Instance == null) yield break;

        IntroManager.Instance.ShowGameOver("<color=red><b>마녀가 당신을 죽였습니다.</b></color>");
        yield return new WaitWhile(() => IntroManager.Instance.IsIntroPlaying);
    }

    public void RecordRoundEndMetrics()
    {
        int correctMemoCount = RoleGuessManager.Instance != null
            ? RoleGuessManager.Instance.CountCorrectGuesses()
            : 0;

        totalCorrectMemoCount += correctMemoCount;

        Debug.Log($"[Score] 스테이지 내 집계 - 플레이한 스테이지 수: {totalStagesPlayed}, 정답 추리 누적: {totalCorrectMemoCount}");
    }

    public void FinalizeScoreAndOpenResult(bool isVictory)
    {
        SyncSacrificeCountFromDeadParticipants();

        ScoreManager.ScoreResult result = ScoreManager.CalculateScore(
            totalStagesPlayed,
            totalSacrificedExcludingWitch,
            totalCorrectMemoCount);

        ScoreRuntimeData.Set(result, isVictory);
        SceneManager.LoadScene("ScoreScene");
    }

    public IEnumerator ShowNightDeathNoticeRoutine()
    {
        if (IntroManager.Instance == null) yield break;

        IntroManager.Instance.ShowNightDeaths(lastNightDeathNames, nightDeathNoticeDuration);
        yield return new WaitWhile(() => IntroManager.Instance.IsIntroPlaying);
    }

    public IEnumerator FadeOutRoutine(float duration)
    {
        if (GlobalFadeManager.Instance == null) yield break;

        bool done = false;
        GlobalFadeManager.Instance.FadeFullOut(duration, () => done = true);
        yield return new WaitUntil(() => done);
    }

    public IEnumerator FadeInRoutine(float duration)
    {
        if (GlobalFadeManager.Instance == null) yield break;

        bool done = false;
        GlobalFadeManager.Instance.FadeFullIn(duration, () => done = true);
        yield return new WaitUntil(() => done);
    }

    private void ResetScoreSession()
    {
        totalStagesPlayed = 0;
        totalSacrificedExcludingWitch = 0;
        totalCorrectMemoCount = 0;
        processedDeadCountInLevel = 0;
    }

    private void SyncSacrificeCountFromDeadParticipants()
    {
        int currentDeadExcludingWitch = 0;
        for (int i = 0; i < deadParticipants.Count; i++)
        {
            CharacterAI dead = deadParticipants[i];
            if (dead == null) continue;
            if (dead.MyRole == Role.Roles.마녀) continue;
            currentDeadExcludingWitch++;
        }

        int delta = currentDeadExcludingWitch - processedDeadCountInLevel;
        if (delta > 0)
        {
            totalSacrificedExcludingWitch += delta;
        }

        processedDeadCountInLevel = currentDeadExcludingWitch;
    }
}
