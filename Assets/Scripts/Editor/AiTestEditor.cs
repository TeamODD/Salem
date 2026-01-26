using UnityEngine;
using UnityEditor; // 에디터 기능 사용

[CustomEditor(typeof(AITestController))]
public class AITestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 속성들을 먼저 그림 (스크립트 필드 등)
        DrawDefaultInspector();

        AITestController script = (AITestController)target;

        GUILayout.Space(10); // 여백 추가
        GUILayout.Label("AI 시스템 테스트 제어", EditorStyles.boldLabel);

        // 가로로 버튼 배치
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("🌙 Run Night", GUILayout.Height(30)))
        {
            script.TestRunNight();
        }

        if (GUILayout.Button("☀️ Run Morning", GUILayout.Height(30)))
        {
            script.TestRunMorning();
        }

        EditorGUILayout.EndHorizontal();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("테스트 버튼은 Play 모드에서만 작동합니다.", MessageType.Info);
        }

        if (GUILayout.Button("🎲 Shuffle Roles", GUILayout.Height(30)))
        {
            script.TestAssignRoles();
        }
    }
}