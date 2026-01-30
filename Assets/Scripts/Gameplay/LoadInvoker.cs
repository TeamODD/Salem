using UnityEngine;
using UnityEngine.Events;

public class LoadInvoker : MonoBehaviour
{
    public UnityEvent OnLoadStart;
    public UnityEvent OnLoadEnd;

    private bool _isLoading = false;
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