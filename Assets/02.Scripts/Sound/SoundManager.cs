using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundType
{
    BGM,
    SFX
}

public class SoundManager : DontDestroySingleton<SoundManager>
{
    [SerializeField] private AudioMixer _audioMixer;

    [SerializeField] private float _currentBGMVolume;
    [SerializeField] private float _currentSFMVolume;

    [SerializeField] private AudioClip[] _preloadedClips;

    private Dictionary<string, AudioClip> _clipDict;
    private List<Sound> _soundList;

    protected override void Awake()
    {
        base.Awake();

        Init();
    }

    private void Init()
    {
        _clipDict = new Dictionary<string, AudioClip>();
        _soundList = new List<Sound>();

        foreach (AudioClip clip in _preloadedClips)
        {
            if (clip != null && !_clipDict.ContainsKey(clip.name))
            {
                _clipDict.Add(clip.name, clip);
            }
        }

        SetVolumes(_currentBGMVolume, _currentSFMVolume);
    }

    public void SetVolumes(float bgmVolume, float sfxVolume)
    {
        SetVolume(SoundType.BGM, bgmVolume);
        SetVolume(SoundType.SFX, sfxVolume);
    }

    public void SetVolume(SoundType type, float value)
    {
        _audioMixer.SetFloat(type.ToString(), value);
    }

    private AudioClip GetClip(string clipName)
    {
        if (_clipDict.TryGetValue(clipName, out AudioClip clip))
        {
            return clip;
        }
        Debug.LogWarning($"{name}:'{clipName}' 클립은 없습니다.");
        return null;
    }

    public void PlayGlobalSound(string clipName, SoundType type = SoundType.SFX, float delay = 0f, bool isLoop = false)
    {
        GameObject soundObject = new GameObject($"Sound_{clipName}");
        Sound sound = soundObject.AddComponent<Sound>();

        if (isLoop)
        {
            _soundList.Add(sound);
        }

        sound.InitGlobalClip(GetClip(clipName));
        sound.Play(_audioMixer.FindMatchingGroups(type.ToString())[0], delay, isLoop);
    }

    public void PlayGlobalRandomSound(string clipName, int min, int max, SoundType type = SoundType.SFX, float delay = 0f, bool isLoop = false)
    {
        string randomClipName = $"{clipName}_{Random.Range(min, max + 1)}";
        PlayGlobalSound(randomClipName, type, delay, isLoop);
    }

    public void PlayLocalSound(string clipName, Transform audioTarget, float delay = 0f, bool isLoop = false, SoundType type = SoundType.SFX, bool attachToTarget = true, float minDistance = 0.0f, float maxDistance = 50.0f)
    {
        GameObject soundObject = new GameObject("TemporarySoundPlayer 3D");
        soundObject.transform.localPosition = audioTarget.transform.position;

        if (attachToTarget)
        {
            soundObject.transform.parent = audioTarget;
        }

        Sound sound = soundObject.AddComponent<Sound>();

        if (isLoop)
        {
            _soundList.Add(sound);
        }

        sound.InitLocalClip(GetClip(clipName), minDistance, maxDistance);
        sound.Play(_audioMixer.FindMatchingGroups(type.ToString())[0], delay, isLoop);
    }

    public void PlayLocalRandomSound(string clipName, Transform audioTarget, int min, int max, float delay = 0f, bool isLoop = false, SoundType type = SoundType.SFX, bool attachToTarget = true, float minDistance = 0.0f, float maxDistance = 50.0f)
    {
        string randomClipName = $"{clipName}_{Random.Range(min, max + 1)}";
        PlayLocalSound(randomClipName, audioTarget, delay, isLoop, type, attachToTarget, minDistance, maxDistance);
    }

    public void StopLoopSound(string clipName)
    {
        Sound sound = _soundList.Find(s => s.name == $"Sound_{clipName}");

        if (sound != null)
        {
            sound.Stop();
            _soundList.Remove(sound);
            Destroy(sound.gameObject);
            return;
        }

        Debug.LogWarning($"{name}:'{clipName}' 루프 사운드를 찾을 수 없습니다.");
    }
}
