using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ExecutionInvoker : MonoBehaviour, IPointerDownHandler
{
    public UnityEvent<Vector2> OnExecute;
    private bool _isExecuted = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isExecuted)
        {
            Debug.Log(gameObject.name + " 이미 처형됨");
            return;
        }

        if (CursorManager.Instance.IsGunCursor())
        {
            if (BulletManager.Instance.HasBullets())
            {
                _isExecuted = true;
                BulletManager.Instance.Shoot();
                Vector2 objectPosition = transform.position;
                OnExecute.Invoke(objectPosition);
            }
            else
            {
                Debug.Log("총알이 없습니다");
            }
        }
        else {
            Debug.Log("총 모드가 아닙니다");
        }
    }
}
