using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Yarn.Unity;

public class ExecutionManager : MonoBehaviour
{
    public static ExecutionManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int maxBullets = 1;
    [SerializeField] private Texture2D aimCursor; // 장전 시 커서
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero; // 커서의 핫스팟 위치

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI bulletCountText; // 탄환 수를 표시할 UI 텍스트

    [Header("State")]
    [SerializeField] private int currentBullets;
    [SerializeField] private bool isAiming = false;

    private CharacterAI _pendingTarget;
    private DialogueRunner _dialogueRunner;
    private GameManager _gameManager;

    public bool IsAiming => isAiming;
    public int CurrentBullets => currentBullets;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentBullets = maxBullets;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateBulletUI();

        // 코드에서 직접 명령어를 등록하여 오브젝트 이름 문제를 해결합니다.
        _dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        _gameManager = FindFirstObjectByType<GameManager>();
        if (_dialogueRunner != null)
        {
            _dialogueRunner.AddCommandHandler("execute_pending", ExecutePendingTarget);
        }
    }

    private void Update()
    {
        // 우클릭으로 장전 취소
        if (isAiming && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            ToggleAiming(false);
        }
    }

    public void ToggleAiming()
    {
        if (currentBullets <= 0)
        {
            Debug.Log("은탄이 부족합니다!");
            return;
        }

        ToggleAiming(!isAiming);
    }

    public void ToggleAiming(bool state)
    {
        isAiming = state;

        if (isAiming)
        {
            Cursor.SetCursor(aimCursor, cursorHotspot, CursorMode.ForceSoftware);
#if UNITY_EDITOR
            Debug.Log("장전 완료. 대상을 선택하세요.");
#endif

            // 장전 완료 시 사운드 효과
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PauseBGM();
                SoundManager.Instance.PlaySFXLoop(SFXType.Heartbeat);
                SoundManager.Instance.PlaySFX(SFXType.LoadPistol);
            }
            if (TunnelVisionEffect.Instance != null)
                TunnelVisionEffect.Instance.StartTunnelVision();
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
#if UNITY_EDITOR
            Debug.Log("장전 해제.");
#endif

            // 장전 해제 시 사운드 효과
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.FadeSFXLoop();
                SoundManager.Instance.ResumeBGM();
            }
            if (TunnelVisionEffect.Instance != null)
                TunnelVisionEffect.Instance.ResetVision();
        }
    }

    public void ExecuteTarget(CharacterAI target, bool forceExecute = false)
    {
        if ((!isAiming && !forceExecute) || currentBullets <= 0) return;
#if UNITY_EDITOR
        Debug.Log($"<color=red>탕! {target.name}을(를) 처형했습니다.</color>");
#endif
        // 1. 탄환 소모
        currentBullets--;
        UpdateBulletUI(); // UI 업데이트
        
        if (isAiming) ToggleAiming(false); // 발사 후 장전 해제 (이미 해제된 상태면 무시)

        // 2. 처형 효과
        if (ExecutionEffect.Instance != null)
        {
            ExecutionEffect.Instance.ExecuteEffect(target.transform.position);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.FadeSFXLoop();
            SoundManager.Instance.PlaySFX(SFXType.GunShot);
            SoundManager.Instance.PlaySFX(SFXType.BloodSpatter);
        }

        // 3. GameManager에게 통보 (사망 처리 및 승리 체크)
        if (_gameManager == null)
        {
            _gameManager = FindFirstObjectByType<GameManager>();
        }

        if (_gameManager != null)
        {
            _gameManager.OnCharacterExecuted(target);
        }
        else
        {
            target.gameObject.SetActive(false);
        }
    }

    public void SetPendingTarget(CharacterAI target) => _pendingTarget = target;

    public void ExecutePendingTarget()
    {
        if (_pendingTarget != null)
        {
            // forceExecute를 true로 보내서 조준 상태가 아니어도 발사되게 함
            ExecuteTarget(_pendingTarget, true);
            _pendingTarget = null;
        }
    }

    private void UpdateBulletUI()
    {
        if (bulletCountText != null)
        {
            bulletCountText.text = $"X {currentBullets}";
        }
    }

    /// <summary>
    /// 스테이지 전환 시 상태 초기화 (탄환 복구, 조준 해제)
    /// </summary>
    public void ResetState()
    {
        currentBullets = maxBullets;
        ToggleAiming(false);
        UpdateBulletUI();
    }
}
