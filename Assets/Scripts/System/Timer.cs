using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Timer : MonoBehaviour
{
    public static Timer Instance;
    public Slider TimerSlider;
    public float GameTime;
    public float ShakeMagnitude = 2f;

    private bool _stopTimer;
    private bool _isShaking;
    private float _currentTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _stopTimer = true;
        _isShaking = false;

        TimerSlider.maxValue = GameTime;
        TimerSlider.value = 0;
        _currentTime = GameTime;
    }

    public System.Action OnTimeUp;

    void Update()
    {
        if (IntroManager.Instance != null && IntroManager.Instance.IsIntroPlaying) return;

        if (_stopTimer || _currentTime <= 0) return;

        _currentTime -= Time.deltaTime;
        TimerSlider.value = GameTime - _currentTime;

        if (_currentTime <= 0)
        {
            _stopTimer = true; // Stop timer explicitly
            StopShake();
            TimerSlider.value = GameTime;
            OnTimeUp?.Invoke(); // Trigger event
        }
        else if (_currentTime <= 10f && !_isShaking)
        {
            StartShake();
        }
    }

    private void StartShake()
    {
        _isShaking = true;

        TimerSlider.transform.DOShakePosition(1f, ShakeMagnitude, 10, 90, false, false)
            .SetLoops(-1)
            .SetId("TimerShake");
    }

    private void StopShake()
    {
        if (!_isShaking) return;

        DOTween.Kill("TimerShake", true);
        _isShaking = false;
    }

    public void StopTimer()
    {
        _stopTimer = true;
        StopShake();
    }

    public void ResetTimer()
    {
        _currentTime = GameTime;
        _stopTimer = false;
        TimerSlider.value = 0;
        StopShake();
    }

    public void ResetTimerPaused()
    {
        _currentTime = GameTime;
        _stopTimer = true;
        TimerSlider.value = 0;
        StopShake();
    }

    public void FinishImmediately()
    {
        if (_stopTimer) return;

        _currentTime = 0;
        _stopTimer = true;
        StopShake();
        OnTimeUp?.Invoke();
    }

    public void ExpireWithoutFill()
    {
        if (_stopTimer) return;

        _stopTimer = true;
        StopShake();
        OnTimeUp?.Invoke();
    }
}
