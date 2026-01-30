using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;

    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    private const string MasterParam = "MasterVol";
    private const string BGMParam = "BGMVol";
    private const string SFXParam = "SFXVol";

    private void Start()
    {
        LoadAndSetVolume(_masterSlider, MasterParam);
        LoadAndSetVolume(_bgmSlider, BGMParam);
        LoadAndSetVolume(_sfxSlider, SFXParam);

        _masterSlider.onValueChanged.AddListener(value => UpdateVolume(MasterParam, value));
        _bgmSlider.onValueChanged.AddListener(value => UpdateVolume(BGMParam, value));
        _sfxSlider.onValueChanged.AddListener(value => UpdateVolume(SFXParam, value));
    }

    private void LoadAndSetVolume(Slider slider, string param)
    {
        float savedValue = PlayerPrefs.GetFloat(param, 0.75f);

        slider.value = savedValue;
        UpdateVolume(param, savedValue);
    }
    
    private void UpdateVolume(string param, float value)
    {
        float dB = Mathf.Log10(Mathf.Max(0.0001f, value)) * 20f;

        _audioMixer.SetFloat(param, dB);
        PlayerPrefs.SetFloat(param, value);
    }
}
