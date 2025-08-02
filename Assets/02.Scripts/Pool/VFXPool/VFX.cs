using UnityEngine;

public class VFX : MonoBehaviour
{
    private ParticleSystem _vfx;

    private void Awake()
    {
        _vfx = GetComponent<ParticleSystem>();
    }

    public void Play()
    {
        _vfx.Play();
    }

    private void OnParticleSystemStopped()
    {
        VFXPool.Instance.Return(this);
    }
}
