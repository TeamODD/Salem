using UnityEngine;

public class AITestController : MonoBehaviour
{
    private AIManager manager;

    private void Awake()
    {
        manager = GetComponent<AIManager>();
    }

    // 인스펙터 버튼으로 호출될 밤 실행 메서드
    public void TestRunNight()
    {
        if (Application.isPlaying)
        {
            Debug.Log("<color=blue><b>[TEST] 밤 로직 실행</b></color>");
            manager.RunNight();
            ShowNightResults();
        }
        else
        {
            Debug.LogWarning("게임 실행(Play) 중에만 테스트 가능합니다.");
        }
    }

    // 인스펙터 버튼으로 호출될 아침 실행 메서드
    public void TestRunMorning()
    {
        if (Application.isPlaying)
        {
            Debug.Log("<color=orange><b>[TEST] 아침 로직 실행</b></color>");
            manager.RunMorning();
        }
        else
        {
            Debug.LogWarning("게임 실행(Play) 중에만 테스트 가능합니다.");
        }
    }

    private void ShowNightResults()
    {
        if (manager.CurrentContext == null) return;

        Debug.Log("--- 밤 행동 결과 보고 ---");
        foreach (var ai in manager.CurrentContext.Participants)
        {
            string action = ai.LastAction != null ? ai.LastAction.ActionId : "행동 없음";
            Debug.Log($"<b>[{ai.MyRole}]</b>: {action}");

            foreach (var line in ai.NightDialogues)
            {
                Debug.Log($"   ㄴ 대사: {line}");
            }
        }
    }
    
    public void TestAssignRoles()
{
    if (Application.isPlaying)
    {
        manager.AssignRandomRoles();
    }
}
}