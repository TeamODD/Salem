using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameManager : MonoBehaviour, IGameFlowContext
{
    public static GameManager Instance { get; private set; }

    [Header("Character Objects")]
    [SerializeField] private List<GameObject> characterObjects = new List<GameObject>();

    [Header("Level Info")]
    [SerializeField] private List<LevelData> levels = new List<LevelData>();
    [SerializeField] private int currentLevelIndex = 0;

    [Header("Round Info")]
    [SerializeField] private List<Role.Roles> activeRoles = new List<Role.Roles>();
    [SerializeField] private int nightIndex = 1;
    [SerializeField] private bool hasEmptyHouseForThief;

    [Header("Flow Timing")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float introLeadDelay = 0.5f;
    [SerializeField] private float nightResolveDelay = 2.0f;

    private bool isNight;
    private bool isTransitioning;

    private readonly List<CharacterAI> participants = new List<CharacterAI>();
    private readonly List<CharacterAI> deadParticipants = new List<CharacterAI>();
    private readonly RoleAssigner roleAssigner = new RoleAssigner();

    private AIContext currentContext;
    private IGameFlowState currentState;
    private Coroutine stateRoutine;

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
        Timer.Instance.FinishImmediately();
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

    public void ResetTimer()
    {
        Timer.Instance?.ResetTimer();
    }

    public void SetFadeOpaque()
    {
        GlobalFadeManager.Instance?.SetAlpha(1.0f);
    }

    public void ShowDefeatUI()
    {
        IntroManager.Instance?.ShowGameOver("<color=red><b>마녀가 당신을 죽였습니다.</b></color>");
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
}
