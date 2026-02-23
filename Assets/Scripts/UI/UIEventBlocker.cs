using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventBlocker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool IsPointerOverUI { get; private set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPointerOverUI = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOverUI = false;
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 상태 초기화
        IsPointerOverUI = false;
    }
}
