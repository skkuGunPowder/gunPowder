using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class Sound : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
    }

    public void InitGlobalClip(AudioClip clip)
    {
        _clip = clip;
        _audioSource.clip = _clip;
    }

    public void InitLocalClip(AudioClip clip, float minDistance = 1f, float maxDistance = 50f)
    {
        _clip = clip;
        _audioSource.clip = _clip;
        _audioSource.spatialBlend = 1f;
        _audioSource.rolloffMode = AudioRolloffMode.Linear;
        _audioSource.minDistance = minDistance;
        _audioSource.maxDistance = maxDistance;
    }

    public void Play(AudioMixerGroup audioMixer, float delay, bool isLoop)
    {
        _audioSource.outputAudioMixerGroup = audioMixer;
        _audioSource.loop = isLoop;
        _audioSource.Play();

        if (!isLoop)
        {
            Destroy(gameObject, _clip.length + delay);
        }
    }

    public void Stop()
    {
        _audioSource.Stop();
    }
}
