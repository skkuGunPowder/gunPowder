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

    private int _originalResolutionIndex;
    private EFullscreenMode _originalFullscreenMode;
    private readonly string[] presetLabels = new string[]
    {
        "1280x720 (HD)",
        "1920x1080 (FHD)",
        "2560x1440 (QHD)"
    };

    private void OnEnable()
    {
        // Snapshot current applied state when popup opens (SetActive true)
        _originalResolutionIndex = SettingManager.Instance.GetCurrentResolutionIndex();
        _originalFullscreenMode = SettingManager.Instance.GetCurrentFullscreenMode();

        InitializeResolutionDropdown();
        InitializeFullscreenToggles();
        InitializeSoundSliders();
    }

    private void UpdateToggleInteractables(EFullscreenMode activeMode)
    {
        if (_windowedToggle == null || _fullscreenToggle == null || _borderlessToggle == null)
        {
            return;
        }

        _windowedToggle.interactable = activeMode != EFullscreenMode.Windowed;
        _fullscreenToggle.interactable = activeMode != EFullscreenMode.Fullscreen;
        _borderlessToggle.interactable = activeMode != EFullscreenMode.Borderless;
    }

    private void UpdateResolutionDropdownInteractable(EFullscreenMode activeMode)
    {
        if (_resolutionDropdown == null)
        {
            return;
        }
        _resolutionDropdown.interactable = (activeMode != EFullscreenMode.Fullscreen);
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

        int currentIndex = SettingManager.Instance.GetCurrentResolutionIndex();
        currentIndex = Mathf.Clamp(currentIndex, 0, presetCount - 1);
        _resolutionDropdown.value = currentIndex;
        _resolutionDropdown.RefreshShownValue();
    }

    private void InitializeFullscreenToggles()
    {
        if (_windowedToggle == null || _fullscreenToggle == null || _borderlessToggle == null)
        {
            return;
        }

        // 현재 적용된 모드에 따라 토글 상태 설정
        EFullscreenMode currentMode = SettingManager.Instance.GetCurrentFullscreenMode();
        
        _windowedToggle.isOn = (currentMode == EFullscreenMode.Windowed);
        _fullscreenToggle.isOn = (currentMode == EFullscreenMode.Fullscreen);
        _borderlessToggle.isOn = (currentMode == EFullscreenMode.Borderless);

        UpdateToggleInteractables(currentMode);
        UpdateResolutionDropdownInteractable(currentMode);
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
        EFullscreenMode currentMode = SettingManager.Instance.GetCurrentFullscreenMode();
        SettingManager.Instance.ApplyResolution(selectedIndex, currentMode, save: false);
    }

    // 인스펙터에서 Windowed Toggle의 OnValueChanged에 할당할 메서드
    public void OnWindowedToggleChanged()
    {
        if (_windowedToggle == null || _windowedToggle.isOn == false)
        {
            return;
        }

        int currentResolution = _resolutionDropdown.value;
        SettingManager.Instance.ApplyResolution(currentResolution, EFullscreenMode.Windowed, save: false);

        UpdateToggleInteractables(EFullscreenMode.Windowed);
        UpdateResolutionDropdownInteractable(EFullscreenMode.Windowed);
    }

    // 인스펙터에서 Fullscreen Toggle의 OnValueChanged에 할당할 메서드
    public void OnFullscreenToggleChanged()
    {
        if (_fullscreenToggle == null || _fullscreenToggle.isOn == false)
        {
            return;
        }

        int currentResolution = _resolutionDropdown.value;
        SettingManager.Instance.ApplyResolution(currentResolution, EFullscreenMode.Fullscreen, save: false);

        UpdateToggleInteractables(EFullscreenMode.Fullscreen);
        UpdateResolutionDropdownInteractable(EFullscreenMode.Fullscreen);
    }

    // 인스펙터에서 Borderless Toggle의 OnValueChanged에 할당할 메서드
    public void OnBorderlessToggleChanged()
    {
        if (_borderlessToggle == null || _borderlessToggle.isOn == false)
        {
            return;
        }

        int currentResolution = _resolutionDropdown.value;
        SettingManager.Instance.ApplyResolution(currentResolution, EFullscreenMode.Borderless, save: false);

        UpdateToggleInteractables(EFullscreenMode.Borderless);
        UpdateResolutionDropdownInteractable(EFullscreenMode.Borderless);
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

    // 인스펙터에서 확인 버튼에 할당할 메서드
    public void OnClickConfirm()
    {
        // 현재 적용된 상태를 저장
        SettingManager.Instance.SaveCurrentResolutionAndMode();
        gameObject.SetActive(false);
    }

    // 인스펙터에서 취소 버튼에 할당할 메서드
    public void OnClickCancel()
    {
        // 원래 상태로 되돌림 (저장하지 않음)
        SettingManager.Instance.ApplyResolution(_originalResolutionIndex, _originalFullscreenMode, save: false);

        // UI도 원래 상태를 반영
        if (_resolutionDropdown != null)
        {
            _resolutionDropdown.value = _originalResolutionIndex;
            _resolutionDropdown.RefreshShownValue();
        }

        if (_windowedToggle != null && _fullscreenToggle != null && _borderlessToggle != null)
        {
            _windowedToggle.isOn = (_originalFullscreenMode == EFullscreenMode.Windowed);
            _fullscreenToggle.isOn = (_originalFullscreenMode == EFullscreenMode.Fullscreen);
            _borderlessToggle.isOn = (_originalFullscreenMode == EFullscreenMode.Borderless);
            UpdateToggleInteractables(_originalFullscreenMode);
        }
        UpdateResolutionDropdownInteractable(_originalFullscreenMode);
        gameObject.SetActive(false);
    }
}
