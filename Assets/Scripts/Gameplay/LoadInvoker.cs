using UnityEngine;
using UnityEngine.Events;

public class LoadInvoker : MonoBehaviour
{
    public UnityEvent OnLoadStart;
    public UnityEvent OnLoadEnd;

    private bool _isLoading = false;

    private void Update()
    {
        // 장전 중일 때 ExecutionManager의 조준이 해제되면 자동으로 StopLoad 호출
        if (_isLoading && ExecutionManager.Instance != null && !ExecutionManager.Instance.IsAiming)
        {
            StopLoad();
        }
    }

    public void ChangeLoadState()
    {
        if (!_isLoading)
            StartLoad();
        else
            StopLoad();
    }

    public void StartLoad()
    {
        _isLoading = true;
        OnLoadStart?.Invoke();
    }

    public void StopLoad()
    {
        _isLoading = false;
        OnLoadEnd?.Invoke();
    }
}
