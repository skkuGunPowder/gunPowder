using System.Collections.Generic;
using UnityEngine;

public class VFX : MonoBehaviour
{
    [SerializeField] private List<AudioClip> VFXSoundClips;
    protected ParticleSystem _vfx;
    
    // Pool key to map this instance back to its prefab queue
    public string PoolKey { get; set; }

    protected virtual void Awake()
    {
        _vfx = GetComponent<ParticleSystem>();
    }

    public void Play()
    {
        if (_vfx == null)
            return;

        // Ensure a clean restart when reused from pool
        _vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _vfx.Play();

        if(VFXSoundClips.Count > 0)
        {
            SoundManager.Instance.PlayLocalRandomSound(VFXSoundClips[0].name.Split('_')[0], transform, 1, VFXSoundClips.Count);
        }
    }

    protected virtual void OnParticleSystemStopped()
    {
        VFXPool.Instance.Return(this);
    }
}
