using UnityEngine;
using System.Collections;

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

    private int _currentResolutionIndex;
    private EFullscreenMode _currentFullscreenMode;

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

        // Borderless(진짜 보더리스 창모드)는 원하는 크기 유지

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
                unityMode = FullScreenMode.Windowed; // 먼저 창 모드로 만들고 스타일 제거
                break;
            default:
                unityMode = FullScreenMode.FullScreenWindow;
                break;
        }

#if UNITY_STANDALONE_WIN
        // 보더리스가 아닌 모드로 전환 시, 기존 보더리스 스타일 복구
        if (fullscreenMode != EFullscreenMode.Borderless)
        {
            WindowsBorderless.RestoreStandardWindow();
        }
#endif

        // 모드 전환 시 대기 중인 코루틴 정리
#if UNITY_STANDALONE_WIN
        if (fullscreenMode != EFullscreenMode.Borderless && _applyBorderlessCoroutine != null)
        {
            StopCoroutine(_applyBorderlessCoroutine);
            _applyBorderlessCoroutine = null;
        }
#endif
        if (fullscreenMode != EFullscreenMode.Windowed && _applyWindowedCoroutine != null)
        {
            StopCoroutine(_applyWindowedCoroutine);
            _applyWindowedCoroutine = null;
        }

        Screen.SetResolution(size.x, size.y, unityMode);

        // 해상도 적용 직후, Windows라면 보더리스 적용
        if (fullscreenMode == EFullscreenMode.Borderless)
        {
#if UNITY_STANDALONE_WIN
            // 해상도/모드 전환 직후에는 아직 윈도우 스타일 적용이 완료되지 않았을 수 있으므로 다음 프레임에 적용
            if (_applyBorderlessCoroutine != null)
            {
                StopCoroutine(_applyBorderlessCoroutine);
                _applyBorderlessCoroutine = null;
            }
            _applyBorderlessCoroutine = StartCoroutine(ApplyBorderlessNextFrame(size));
#endif
        }
        else if (fullscreenMode == EFullscreenMode.Windowed)
        {
            // 전체화면 → 윈도우 전환 시 선택된 해상도를 다음 프레임에 재적용
            if (_applyWindowedCoroutine != null)
            {
                StopCoroutine(_applyWindowedCoroutine);
                _applyWindowedCoroutine = null;
            }
            _applyWindowedCoroutine = StartCoroutine(ApplyWindowedNextFrame(size));
        }

        // Track current applied state regardless of save flag
        _currentResolutionIndex = clamped;
        _currentFullscreenMode = fullscreenMode;

        if (save)
        {
            PlayerPrefs.SetInt(PrefKeyResolutionIndex, clamped);
            PlayerPrefs.SetInt(PrefKeyFullscreenMode, (int)fullscreenMode);
            PlayerPrefs.Save();
        }
    }

    private Coroutine _applyWindowedCoroutine;

#if UNITY_STANDALONE_WIN
    private Coroutine _applyBorderlessCoroutine;

    private IEnumerator ApplyBorderlessNextFrame(Vector2Int size)
    {
        yield return null; // 다음 프레임까지 대기
        yield return new WaitForEndOfFrame();

        int x = (Display.main.systemWidth - size.x) / 2;
        int y = (Display.main.systemHeight - size.y) / 2;
        WindowsBorderless.MakeBorderless(x, y, size.x, size.y);

        _applyBorderlessCoroutine = null;
    }
#endif

    private IEnumerator ApplyWindowedNextFrame(Vector2Int size)
    {
        yield return null; // 다음 프레임까지 대기
        yield return new WaitForEndOfFrame();

        Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);

        _applyWindowedCoroutine = null;
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

    public int GetCurrentResolutionIndex()
    {
        return _currentResolutionIndex;
    }

    public EFullscreenMode GetCurrentFullscreenMode()
    {
        return _currentFullscreenMode;
    }

    public void SaveCurrentResolutionAndMode()
    {
        int clamped = Mathf.Clamp(_currentResolutionIndex, 0, ResolutionPresets.Length - 1);
        PlayerPrefs.SetInt(PrefKeyResolutionIndex, clamped);
        PlayerPrefs.SetInt(PrefKeyFullscreenMode, (int)_currentFullscreenMode);
        PlayerPrefs.Save();
    }
}

