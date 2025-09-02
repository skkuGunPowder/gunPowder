using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UI_SettingPopup : UI_Popup
{
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    
    [Header("화면 모드 토글")]
    [SerializeField] private Toggle _windowedToggle;
    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private Toggle _borderlessToggle;

    [Header("사운드 볼륨")]
    [SerializeField] private Slider _bgmVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI _bgmVolumeText;
    [SerializeField] private TextMeshProUGUI _sfxVolumeText;


    private readonly string[] presetLabels = new string[]
    {
        "1280x720 (HD)",
        "1920x1080 (FHD)",
        "2560x1440 (QHD)"
    };

    private void OnEnable()
    {
        InitializeResolutionDropdown();
        InitializeFullscreenToggles();
        InitializeSoundSliders();
    }

    private void InitializeResolutionDropdown()
    {
        if (_resolutionDropdown == null)
        {
            return;
        }

        _resolutionDropdown.ClearOptions();

        int presetCount = SettingManager.Instance.GetPresetCount();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>(presetCount);
        for (int i = 0; i < presetCount; i++)
        {
            string label = i < presetLabels.Length ? presetLabels[i] : SettingManager.Instance.GetPresetResolution(i).x + "x" + SettingManager.Instance.GetPresetResolution(i).y;
            options.Add(new TMP_Dropdown.OptionData(label));
        }
        _resolutionDropdown.AddOptions(options);

        int savedIndex = SettingManager.Instance.GetSavedResolutionIndex();
        savedIndex = Mathf.Clamp(savedIndex, 0, presetCount - 1);
        _resolutionDropdown.value = savedIndex;
        _resolutionDropdown.RefreshShownValue();
    }

    private void InitializeFullscreenToggles()
    {
        if (_windowedToggle == null || _fullscreenToggle == null || _borderlessToggle == null)
        {
            return;
        }

        // 현재 저장된 모드에 따라 토글 상태 설정
        EFullscreenMode savedMode = SettingManager.Instance.GetSavedFullscreenMode();
        
        _windowedToggle.isOn = (savedMode == EFullscreenMode.Windowed);
        _fullscreenToggle.isOn = (savedMode == EFullscreenMode.Fullscreen);
        _borderlessToggle.isOn = (savedMode == EFullscreenMode.Borderless);
    }

    private void InitializeSoundSliders()
    {
        if (_bgmVolumeSlider == null || _sfxVolumeSlider == null)
        {
            return;
        }

        // 저장된 볼륨 값으로 슬라이더 초기화
        float savedBGMVolume = SettingManager.Instance.GetSavedBGMVolume();
        float savedSFXVolume = SettingManager.Instance.GetSavedSFXVolume();

        _bgmVolumeSlider.value = savedBGMVolume;
        _sfxVolumeSlider.value = savedSFXVolume;
        _bgmVolumeText.text = Mathf.Round(savedBGMVolume * 100).ToString() + "%";
        _sfxVolumeText.text = Mathf.Round(savedSFXVolume * 100).ToString() + "%";
    }

    // 인스펙터에서 TMP_Dropdown의 OnValueChanged에 할당할 메서드
    public void OnResolutionChanged()
    {
        // 선택된 인덱스 받아오기
        int selectedIndex = _resolutionDropdown.value;

        EFullscreenMode currentMode = SettingManager.Instance.GetSavedFullscreenMode();
        SettingManager.Instance.ApplyResolution(selectedIndex, currentMode, save: true);
    }

    // 인스펙터에서 Windowed Toggle의 OnValueChanged에 할당할 메서드
    public void OnWindowedToggleChanged()
    {
        _fullscreenToggle.isOn = false;
        _borderlessToggle.isOn = false;
        
        int currentResolution = _resolutionDropdown.value;
        SettingManager.Instance.ApplyResolution(currentResolution, EFullscreenMode.Windowed, save: true);
    }

    // 인스펙터에서 Fullscreen Toggle의 OnValueChanged에 할당할 메서드
    public void OnFullscreenToggleChanged()
    {
        _windowedToggle.isOn = false;
        _borderlessToggle.isOn = false;
        
        int currentResolution = _resolutionDropdown.value;
        SettingManager.Instance.ApplyResolution(currentResolution, EFullscreenMode.Fullscreen, save: true);
    }

    // 인스펙터에서 Borderless Toggle의 OnValueChanged에 할당할 메서드
    public void OnBorderlessToggleChanged()
    {
        _windowedToggle.isOn = false;
        _fullscreenToggle.isOn = false;
        
        int currentResolution = _resolutionDropdown.value;
        SettingManager.Instance.ApplyResolution(currentResolution, EFullscreenMode.Borderless, save: true);
        
    }

    // 인스펙터에서 Volume Slider의 OnValueChanged에 할당할 메서드
    public void OnVolumeChanged()
    {
        float currentSFXVolume = _sfxVolumeSlider.value;
        float currentBGMVolume = _bgmVolumeSlider.value;
        SettingManager.Instance.ApplySoundVolumes(currentBGMVolume, currentSFXVolume, save: true);
        _bgmVolumeText.text = Mathf.Round(currentBGMVolume * 100).ToString() + "%";
        _sfxVolumeText.text = Mathf.Round(currentSFXVolume * 100).ToString() + "%";
    }
}
