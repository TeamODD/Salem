using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class ExecutionManager : MonoBehaviour
{
    public static ExecutionManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int maxBullets = 1;
    [SerializeField] private Texture2D aimCursor; // 장전 시 커서
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;

    [Header("State")]
    [SerializeField] private int currentBullets;
    [SerializeField] private bool isAiming = false;

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
            Cursor.SetCursor(aimCursor, cursorHotspot, CursorMode.Auto);
#if UNITY_EDITOR
            Debug.Log("장전 완료. 대상을 선택하세요.");
#endif
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
#if UNITY_EDITOR
            Debug.Log("장전 해제.");
#endif
        }
    }

    public void ExecuteTarget(CharacterAI target)
    {
        if (!isAiming || currentBullets <= 0) return;
#if UNITY_EDITOR
        Debug.Log($"<color=red>탕! {target.name}을(를) 처형했습니다.</color>");
#endif
        // 1. 탄환 소모
        currentBullets--;
        ToggleAiming(false); // 발사 후 장전 해제

        // 2. 처형 효과 (피 튀김 등) - 추후 구현 or EffectManager 호출
        // EffectManager.Instance.ShowBloodEffect(target.transform.position);

        // 3. AIManager에게 통보 (사망 처리 및 승리 체크)
        AIManager aiManager = FindFirstObjectByType<AIManager>();
        if (aiManager != null)
        {
            aiManager.OnCharacterExecuted(target);
        }
        else
        {
            target.gameObject.SetActive(false);
        }
    }
}
