using UnityEngine;

// VFX 원본은 그대로 두고, 따라다니는 기능만 추가한 파생 클래스
public class FollowVFX : VFX
{
    private Transform _target;

    /// <summary>
    /// 타겟에 부착한 뒤 재생한다. 파티클 종료 시 비활성화되면서 자동으로 분리됨.
    /// </summary>
    public void PlayAttached(Transform target, bool keepWorldPosition = false, Vector3 offset = default)
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
        transform.localPosition = offset;
        Play();
    }
    
    /// <summary>
    /// 타겟 위쪽으로 호를 그리는 랜덤 오프셋을 생성하여 부착한 뒤 재생
    /// </summary>
    /// <param name="target">따라다닐 타겟</param>
    /// <param name="arcRadius">호의 반지름</param>
    /// <param name="arcAngleRange">호의 각도 범위 (0~180, 0도=우측, 90도=위, 180도=좌측)</param>
    public void PlayAttachedWithArcOffset(Transform target, float arcRadius = 1f, float arcAngleRange = 90f)
    {
        // 위쪽 방향 호 안의 랜덤 위치 생성
        // arcAngleRange를 좌우로 균등 분배 (예: 90도 → 45도~135도)
        float minAngle = 90f - (arcAngleRange / 2f);
        float maxAngle = 90f + (arcAngleRange / 2f);
        
        // 랜덤 각도와 거리
        float randomAngle = Random.Range(minAngle, maxAngle);
        float randomDistance = Random.Range(0f, arcRadius);
        
        // 각도를 라디안으로 변환 (Unity는 오른쪽이 0도)
        float angleInRadians = randomAngle * Mathf.Deg2Rad;
        
        // 2D 평면에서 오프셋 계산 (Z축은 0)
        Vector3 offset = new Vector3(
            Mathf.Cos(angleInRadians) * randomDistance,
            Mathf.Sin(angleInRadians) * randomDistance,
            0f
        );
        
        PlayAttached(target, false, offset);
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


