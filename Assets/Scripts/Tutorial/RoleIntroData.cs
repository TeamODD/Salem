using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 단일 역할 정보
/// </summary>
[Serializable]
public class RoleEntry
{
    public string RoleName = "";
    public Sprite RoleIcon;
    [TextArea(3, 10)]
    public string RoleDescription = "";
}

/// <summary>
/// 모든 역할 정보를 저장하는 마스터 데이터베이스 (ScriptableObject)
/// </summary>
[CreateAssetMenu(fileName = "RoleIntroData", menuName = "Tutorial/Role Intro Data")]
public class RoleIntroData : ScriptableObject
{
    [Tooltip("모든 역할 정보 목록")]
    public List<RoleEntry> AllRoles = new List<RoleEntry>();

    /// <summary>
    /// 역할 이름으로 RoleEntry를 검색
    /// </summary>
    public RoleEntry GetRole(string roleName)
    {
        return AllRoles.Find(r => r.RoleName == roleName);
    }

    /// <summary>
    /// 여러 역할 이름으로 RoleEntry 리스트를 가져옴
    /// </summary>
    public List<RoleEntry> GetRoles(List<string> roleNames)
    {
        List<RoleEntry> result = new List<RoleEntry>();
        foreach (string name in roleNames)
        {
            RoleEntry entry = GetRole(name);
            if (entry != null)
            {
                result.Add(entry);
            }
            else
            {
                Debug.LogWarning($"[RoleIntroData] '{name}' 역할을 찾을 수 없습니다.");
            }
        }
        return result;
    }
}
