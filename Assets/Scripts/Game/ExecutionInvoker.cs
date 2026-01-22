using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ExecutionInvoker : MonoBehaviour, IPointerDownHandler
{
    public UnityEvent<Vector2> OnExecute;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CursorManager.Instance.IsGunCursor())
        {
            Vector2 objectPosition = transform.position;
            OnExecute.Invoke(objectPosition);
        }
    }
}
