using UnityEngine;

public class Character : MonoBehaviour
{
    private Role.Roles _role;

    public void SetUpRole(Role.Roles role)
    {
        _role = role;
        gameObject.SetActive(true);
        Debug.Log("캐릭터 역할 설정됨: " + role.ToString());
    }

    public void Deactive()
    {
        gameObject.SetActive(false);
    }
}
