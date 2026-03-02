using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ActionIdAttribute))]
public class ActionIdDrawer : PropertyDrawer
{
    // 추가하고 싶은 Action ID들을 리스트로 작성
    private readonly string[] actionIds = new string[]
    {
        "None",
        "witch_attack", "witch_pretend",           // 마녀
        "believer_investigate", "believer_stay_home", "believer_fail_empty", // 신자
        "thief_lie", "thief_truth",                // 좀도둑
        "insomniac_walk", "insomniac_home",        // 불면증
        "coward_silent", "mute_silent"             // 기타
    };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            // 현재 선택된 값의 위치(index)를 찾습니다.
            int index = Mathf.Max(0, System.Array.IndexOf(actionIds, property.stringValue));

            // 인스펙터에 드롭다운(Popup)을 그립니다.
            index = EditorGUI.Popup(position, label.text, index, actionIds);

            // 선택한 텍스트를 실제 변수에 저장합니다.
            property.stringValue = actionIds[index];
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "ActionId는 string에서만 쓸 수 있어요.");
        }
    }
}