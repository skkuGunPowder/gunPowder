using System.Collections.Generic;
using UnityEngine;

public class VFX : MonoBehaviour
{
    [SerializeField] private List<AudioClip> VFXSoundClips;
    private ParticleSystem _vfx;

    private void Awake()
    {
        _vfx = GetComponent<ParticleSystem>();
    }

    public void Play()
    {
        _vfx.Play();

        if(VFXSoundClips.Count > 0)
        {
            SoundManager.Instance.PlayLocalRandomSound(VFXSoundClips[0].name.Split('_')[0], transform, 1, VFXSoundClips.Count);
        }
    }

    private void OnParticleSystemStopped()
    {
        VFXPool.Instance.Return(this);
    }
}
