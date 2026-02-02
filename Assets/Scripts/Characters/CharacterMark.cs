using UnityEngine;
using UnityEngine.UI;
public class CharacterMark : MonoBehaviour
{
    [SerializeField] private Image _markImage;
    [SerializeField] private Sprite _defaultMark;

    void Start()
    {
        if (RoleGuessManager.Instance != null)
        {
            RoleGuessManager.Instance.RegisterMark(this);
        }   
    }
    public void OnMarkClicked()
    {
        RoleGuessManager.Instance.OpenSelector(this);
    }

    public void SetGuessedRole(Sprite newIcon)
    {
        _markImage.sprite = newIcon != null ? newIcon : _defaultMark;
    }
}