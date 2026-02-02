using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }

    // 배경음 재생
    public void PlayBGM(AudioClip clip)
    {
        _bgmSource.clip = clip;
        _bgmSource.Play();
    }

    // 효과음 한 번 재생
    public void PlaySFX(AudioClip clip)
    {
        _sfxSource.PlayOneShot(clip);
    }

    public void PauseBGM()
    {
        if (_bgmSource != null && _bgmSource.isPlaying)
        {
            _bgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (_bgmSource != null)
        {
            _bgmSource.UnPause();
        }
    }

    public void StopBGM()
    {
        if (_bgmSource != null && _bgmSource.isPlaying)
        {
            _bgmSource.Stop();
        }
    }
    
    public void StopSFX()
    {
        if (_sfxSource != null && _sfxSource.isPlaying)
        {
            _sfxSource.Stop();
        }
    }   

}