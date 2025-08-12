using UnityEngine;

// VFX 원본은 그대로 두고, 따라다니는 기능만 추가한 파생 클래스
public class FollowVFX : VFX
{
    private Transform _target;

    /// <summary>
    /// 타겟에 부착한 뒤 재생한다. 파티클 종료 시 비활성화되면서 자동으로 분리됨.
    /// </summary>
    public void PlayAttached(Transform target, bool keepWorldPosition = false)
    {
        _target = target;
        if (_target != null)
        {
            transform.SetParent(_target, keepWorldPosition);
            // When following, simulate in Local space so particles move with the target
            if (_vfx != null)
            {
                var main = _vfx.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
            }
        }
        // Reset local position when attaching so it spawns at the target
        transform.localPosition = Vector3.zero;
        Play();
    }

    /// <summary>
    /// 타겟을 교체하거나 분리한다.
    /// </summary>
    public void SetTarget(Transform newTarget, bool keepWorldPosition = false)
    {
        _target = newTarget;
        if (_target != null)
        {
            transform.SetParent(_target, keepWorldPosition);
            if (_vfx != null)
            {
                var main = _vfx.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
            }
            transform.localPosition = Vector3.zero;
        }
    }

    protected override void OnParticleSystemStopped()
    {
        if (_vfx != null)
        {
            var main = _vfx.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
        }
        // Detach to avoid keeping it under player in pool
        transform.SetParent(null, true);
        base.OnParticleSystemStopped();
    }
}


