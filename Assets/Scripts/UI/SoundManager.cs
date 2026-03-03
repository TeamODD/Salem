using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    [Header("오디오 소스")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _loopSfxSource;

    [Header("데이터")]
    [SerializeField] private SoundDatabase _soundDB;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TitleScene")
        {
            PlayBGM(BGMType.Title);
        }
        else if (scene.name == "MainScene")
        {
            PlayBGM(BGMType.Main);
        }
        else if (scene.name == "TutorialScene")
        {
            PlayBGM(BGMType.Tutorial);
        }
    }

    public void PlayBGM(BGMType type)
    {
        if (_soundDB == null) return;
        
        AudioClip clip = _soundDB.GetBGMClip(type);
        if (clip == null) return;

        if (_bgmSource.isPlaying && _bgmSource.clip == clip) return;

        _bgmSource.clip = clip;
        _bgmSource.Play();
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (_bgmSource.isPlaying && _bgmSource.clip == clip) return;

        _bgmSource.clip = clip;
        _bgmSource.Play();
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

    public float GetBGMVolume()
    {
        return _bgmSource != null ? _bgmSource.volume : 0f;
    }

    public void SetBGMVolume(float volume)
    {
        if (_bgmSource != null)
        {
            _bgmSource.volume = volume;
        }
    }

    public void PlaySFX(SFXType type)
    {
        if (_soundDB == null) return;

        AudioClip clip = _soundDB.GetSFXClip(type);
        if (clip != null)
        {
            _sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            _sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFXLoop(AudioClip clip)
    {
        if (_loopSfxSource == null || clip == null) return;

        _loopSfxSource.clip = clip;
        _loopSfxSource.loop = true;
        _loopSfxSource.Play();
    }

    public void PlaySFXLoop(SFXType type)
    {
        if (_soundDB == null) return;

        AudioClip clip = _soundDB.GetSFXClip(type);
        if (clip != null)
        {
            PlaySFXLoop(clip);
        }
    }

    public void FadeSFXLoop()
    {
        if (_loopSfxSource != null && _loopSfxSource.isPlaying)
        {
            _loopSfxSource.loop = false;
        }
    }
    public void StopSFXLoop()
    {
        if (_loopSfxSource != null && _loopSfxSource.isPlaying)
        {
            _loopSfxSource.Stop();
            _loopSfxSource.clip = null;
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