using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class Timer : MonoBehaviour
{
    public Slider TimerSlider; 
    public float GameTime; // 제한 시간
    public float ShakeMagnitude = 2f; // 흔들림 강도
    private bool _stopTimer; 
    private bool _isShaking;
    private Coroutine _shakeCoroutine;
    private Vector3 _originalPosition;

    void Start()
    {
        _stopTimer = false;
        _isShaking = false;
        TimerSlider.maxValue = GameTime;
        TimerSlider.value = GameTime;
        _originalPosition = TimerSlider.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float time = GameTime - Time.time;
        if (time <= 0 || _stopTimer)
        {
            TimerSlider.value = 0;
            if (_isShaking)
            {
                StopCoroutine(_shakeCoroutine);
                _isShaking = false;
                TimerSlider.transform.localPosition = _originalPosition;
            }
        }
        else
        {
            TimerSlider.value = GameTime - time;
            if (time <= 30 && !_isShaking)
            {
                _shakeCoroutine = StartCoroutine(Shake());
            }
        }
    }
    private IEnumerator Shake()
    {
        _isShaking = true;

        while (!_stopTimer && (GameTime - Time.time) > 0)
        {
            float x = Random.Range(-1f, 1f) * ShakeMagnitude;
            float y = Random.Range(-1f, 1f) * ShakeMagnitude;
            TimerSlider.transform.localPosition = _originalPosition + new Vector3(x, y, 0);
            yield return null;
        }

        TimerSlider.transform.localPosition = _originalPosition;
        _isShaking = false;
    }
}
