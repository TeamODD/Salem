using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager Instance { get; private set; }

    [SerializeField] private AudioMixer _audioMixer;

    private Slider _masterSlider;
    private Slider _bgmSlider;
    private Slider _sfxSlider;

    private const string MasterParam = "MasterVol";
    private const string BGMParam = "BGMVol";
    private const string SFXParam = "SFXVol";

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
        // 씬 전환 후 새로운 SettingsPanel의 슬라이더 찾기
        FindAndSetupSliders();
    }

    private void Start()
    {
        // 첫 씬에서 슬라이더 찾기
        FindAndSetupSliders();
    }

    private void FindAndSetupSliders()
    {
        // 이전 슬라이더의 리스너 제거
        RemoveAllListeners();

        // SettingsPanel 찾기 (SettingCanvas/SettingsPanel)
        Transform settingsPanel = GameObject.Find("SettingCanvas/SettingsPanel")?.transform;

        // 찾지 못하면 다른 방법 시도
        if (settingsPanel == null)
        {
            GameObject settingCanvas = GameObject.Find("SettingCanvas");
            if (settingCanvas != null)
            {
                settingsPanel = settingCanvas.transform.Find("SettingsPanel");
            }
        }

        // 슬라이더 찾기 (이름 기반 검색)
        Slider[] allSliders = settingsPanel.GetComponentsInChildren<Slider>();

        foreach (Slider slider in allSliders)
        {
            if (slider.name.Contains("Master"))
                _masterSlider = slider;
            else if (slider.name.Contains("BGM"))
                _bgmSlider = slider;
            else if (slider.name.Contains("SFX"))
                _sfxSlider = slider;
        }

        // 슬라이더 설정
        if (_masterSlider != null)
        {
            LoadAndSetVolume(_masterSlider, MasterParam);
            _masterSlider.onValueChanged.AddListener(value => UpdateVolume(MasterParam, value));
        }

        if (_bgmSlider != null)
        {
            LoadAndSetVolume(_bgmSlider, BGMParam);
            _bgmSlider.onValueChanged.AddListener(value => UpdateVolume(BGMParam, value));
        }

        if (_sfxSlider != null)
        {
            LoadAndSetVolume(_sfxSlider, SFXParam);
            _sfxSlider.onValueChanged.AddListener(value => UpdateVolume(SFXParam, value));
        }
    }

    private void RemoveAllListeners()
    {
        if (_masterSlider != null)
            _masterSlider.onValueChanged.RemoveAllListeners();
        if (_bgmSlider != null)
            _bgmSlider.onValueChanged.RemoveAllListeners();
        if (_sfxSlider != null)
            _sfxSlider.onValueChanged.RemoveAllListeners();
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
