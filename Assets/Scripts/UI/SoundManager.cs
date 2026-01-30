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
       if (clip == null) { Debug.LogError("재생할 클립이 없어요!"); return; }
    Debug.Log($"{clip.name} 재생 시도 중!");
    _sfxSource.PlayOneShot(clip);
    }
}