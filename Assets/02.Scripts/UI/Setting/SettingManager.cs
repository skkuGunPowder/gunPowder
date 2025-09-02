using UnityEngine;

public class SettingManager : DontDestroySingleton<SettingManager>
{
    private const string PrefKeyResolutionIndex = "pref_resolution_index";
    private const string PrefKeyFullscreenMode = "pref_fullscreen_mode"; // 0: Windowed, 1: Fullscreen, 2: Borderless
    private const string PrefKeyBGMVolume = "pref_bgm_volume";
    private const string PrefKeySFXVolume = "pref_sfx_volume";

    // Fixed presets: 1280x720 (HD), 1920x1080 (FHD), 2560x1440 (QHD)
    // Index 0,1,2 respectively
    private static readonly Vector2Int[] ResolutionPresets = new Vector2Int[]
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440)
    };
    
    private void Update()
    {
        // p키 누르면 Playerorefs 초기화
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayerPrefs.DeleteAll();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        // Apply saved settings at startup
        int savedIndex = GetSavedResolutionIndex();
        EFullscreenMode savedMode = GetSavedFullscreenMode();
        ApplyResolution(savedIndex, savedMode, save: false);
    }

    private void Start()
    {
        // Apply saved sound volumes
        float savedBGMVolume = GetSavedBGMVolume();
        float savedSFXVolume = GetSavedSFXVolume();
        ApplySoundVolumes(savedBGMVolume, savedSFXVolume, save: false);
    }

    public int GetPresetCount()
    {
        return ResolutionPresets.Length;
    }

    public Vector2Int GetPresetResolution(int index)
    {
        if (index < 0 || index >= ResolutionPresets.Length)
        {
            index = Mathf.Clamp(index, 0, ResolutionPresets.Length - 1);
        }
        return ResolutionPresets[index];
    }

    public int GetSavedResolutionIndex()
    {
        return PlayerPrefs.GetInt(PrefKeyResolutionIndex, 1); // Default FHD
    }

    public EFullscreenMode GetSavedFullscreenMode()
    {
        return (EFullscreenMode)PlayerPrefs.GetInt(PrefKeyFullscreenMode, (int)EFullscreenMode.Fullscreen); // Default fullscreen
    }

    public float GetSavedBGMVolume()
    {
        return PlayerPrefs.GetFloat(PrefKeyBGMVolume, 1.0f); // Default 100%
    }

    public float GetSavedSFXVolume()
    {
        return PlayerPrefs.GetFloat(PrefKeySFXVolume, 1.0f); // Default 100%
    }

    public void ApplyResolution(int presetIndex, EFullscreenMode fullscreenMode, bool save = true)
    {
        int clamped = Mathf.Clamp(presetIndex, 0, ResolutionPresets.Length - 1);
        Vector2Int size = ResolutionPresets[clamped];

        FullScreenMode unityMode;
        switch (fullscreenMode)
        {
            case EFullscreenMode.Windowed:
                unityMode = FullScreenMode.Windowed;
                break;
            case EFullscreenMode.Fullscreen:
                unityMode = FullScreenMode.FullScreenWindow;
                break;
            case EFullscreenMode.Borderless:
                unityMode = FullScreenMode.MaximizedWindow;
                break;
            default:
                unityMode = FullScreenMode.FullScreenWindow;
                break;
        }

        Screen.SetResolution(size.x, size.y, unityMode);

        if (save)
        {
            PlayerPrefs.SetInt(PrefKeyResolutionIndex, clamped);
            PlayerPrefs.SetInt(PrefKeyFullscreenMode, (int)fullscreenMode);
            PlayerPrefs.Save();
        }
    }

    public void ApplySoundVolumes(float bgmVolume, float sfxVolume, bool save = true)
    {
        // SoundManager를 통해 볼륨 적용
        if (SoundManager.Instance != null)
        {
            float bgmDb = Linear01ToDecibel(bgmVolume);
            float sfxDb = Linear01ToDecibel(sfxVolume);
            SoundManager.Instance.SetVolume(SoundType.BGM, bgmDb);
            SoundManager.Instance.SetVolume(SoundType.SFX, sfxDb);
        }

        if (save)
        {
            PlayerPrefs.SetFloat(PrefKeyBGMVolume, bgmVolume);
            PlayerPrefs.SetFloat(PrefKeySFXVolume, sfxVolume);
            PlayerPrefs.Save();
        }
    }

    private static float Linear01ToDecibel(float linear)
    {
        if (linear <= 0.0001f) return -80f;
        float db = Mathf.Log10(linear) * 20f;
        return Mathf.Clamp(db, -80f, 0f);
    }
}
