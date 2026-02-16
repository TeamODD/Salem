using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Salem/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string LevelName;
    [TextArea] public string IntroText; // 레벨 시작 시 띄울 텍스트 (옵션)
    public List<Role.Roles> RolesInLevel = new List<Role.Roles>();

    [Header("Characters")]
    // 이 레벨에 등장할 캐릭터들의 데이터 (순서대로 1번, 2번... 자리에 배치)
    public List<CharacterData> CharacterDatas = new List<CharacterData>();
}
