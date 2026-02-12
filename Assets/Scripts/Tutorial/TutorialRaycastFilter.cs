using UnityEngine;
using UnityEngine.UI;

public class TutorialRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    private RectTransform _targetRect;
    private Camera _targetCamera; // Camera for the UI target
    
    private Transform _targetWorld;
    private float _worldRadius; 
    private Camera _mainCamera;

    private bool _isWorldTarget;

    /// <summary>
    /// 타겟 설정
    /// </summary>
    /// <param name="target">허용할 타겟 오브젝트</param>
    /// <param name="worldSize">월드 오브젝트일 경우 허용 범위 (지름)</param>
    public void SetTarget(GameObject target, float worldSize = 0f)
    {
        if (target == null)
        {
            Clear();
            return;
        }

        _targetRect = target.GetComponent<RectTransform>();
        if (_targetRect != null)
        {
            // UI 타겟
            _isWorldTarget = false;
            
            Canvas canvas = _targetRect.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                _targetCamera = canvas.worldCamera;
            }
            else
            {
                _targetCamera = null;
            }
        }
        else
        {
            // 월드 타겟
            _isWorldTarget = true;
            _targetWorld = target.transform;
            _worldRadius = worldSize * 0.5f; // 지름 -> 반지름
            if (_mainCamera == null) _mainCamera = Camera.main;
        }
    }

    public void Clear()
    {
        _targetRect = null;
        _targetWorld = null;
        _targetCamera = null;
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (_targetRect == null && _targetWorld == null) return true;

        // 월드 오브젝트 타겟
        if (_isWorldTarget)
        {
            if (_targetWorld == null || _mainCamera == null) return true;
            Vector2 targetScreenPos = _mainCamera.WorldToScreenPoint(_targetWorld.position);
            float dist = Vector2.Distance(sp, targetScreenPos);
            
            return dist > _worldRadius;
        }
        // UI 오브젝트 타겟
        else
        {
            return !RectTransformUtility.RectangleContainsScreenPoint(_targetRect, sp, _targetCamera);
        }
    }
}
