using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Timer : MonoBehaviour
{
    public static Timer Instance;
    private const float MinimumGameTime = 30f;

    public Slider TimerSlider;
    public float GameTime;
    public float ShakeMagnitude = 2f;

    private bool _stopTimer;
    private bool _isShaking;
    private bool _isCountdownLoopPlaying;
    private bool _hasStoredOriginalBgmVolume;
    private float _currentTime;
    private float _originalBgmVolume;

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
        _isCountdownLoopPlaying = false;
        _hasStoredOriginalBgmVolume = false;
        _originalBgmVolume = 0f;

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
            StopCountdownLoop();
            StopShake();
            TimerSlider.value = GameTime;
            OnTimeUp?.Invoke(); // Trigger event
            return;
        }
        else if (_currentTime <= 10f && !_isShaking)
        {
            StartShake();
        }

        if (_currentTime <= 10f && !_isCountdownLoopPlaying)
        {
            _isCountdownLoopPlaying = true;
            LowerBGMForCountdown();
            SoundManager.Instance?.PlaySFXLoop(SFXType.ClockTick);
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
        StopCountdownLoop();
        StopShake();
    }

    public void ResetTimer()
    {
        _currentTime = GameTime;
        _stopTimer = false;
        StopCountdownLoop();
        TimerSlider.value = 0;
        StopShake();
    }

    public void ResetTimerPaused()
    {
        _currentTime = GameTime;
        _stopTimer = true;
        StopCountdownLoop();
        TimerSlider.value = 0;
        StopShake();
    }

    public void AdjustGameTime(float deltaSeconds)
    {
        GameTime = Mathf.Max(MinimumGameTime, GameTime + deltaSeconds);
        _currentTime = Mathf.Min(_currentTime, GameTime);

        if (TimerSlider != null)
        {
            TimerSlider.maxValue = GameTime;
            TimerSlider.value = _stopTimer ? 0f : GameTime - _currentTime;
        }
    }

    public void FinishImmediately()
    {
        if (_stopTimer) return;

        _currentTime = 0;
        _stopTimer = true;
        StopCountdownLoop();
        StopShake();
        OnTimeUp?.Invoke();
    }

    public void ExpireWithoutFill()
    {
        if (_stopTimer) return;

        _stopTimer = true;
        StopCountdownLoop();
        StopShake();
        OnTimeUp?.Invoke();
    }

    private void StopCountdownLoop()
    {
        if (!_isCountdownLoopPlaying) return;

        SoundManager.Instance?.StopSFXLoop();
        RestoreBGMVolume();
        _isCountdownLoopPlaying = false;
    }

    private void LowerBGMForCountdown()
    {
        if (SoundManager.Instance == null || _hasStoredOriginalBgmVolume) return;

        _originalBgmVolume = SoundManager.Instance.GetBGMVolume();
        _hasStoredOriginalBgmVolume = true;
        SoundManager.Instance.SetBGMVolume(_originalBgmVolume * 0.5f);
    }

    private void RestoreBGMVolume()
    {
        if (SoundManager.Instance == null || !_hasStoredOriginalBgmVolume) return;

        SoundManager.Instance.SetBGMVolume(_originalBgmVolume);
        _hasStoredOriginalBgmVolume = false;
    }
}
