using RaycastPro.RaySensors2D;
using UnityEngine;

public class DummyMove : Dummy
{
    private float _timer = 0f;
    private bool _hit = false;
    private bool _isFall = false;
    
    public float HitTime = 0.25f;

    private void Update()
    {
        if (_hit)
        {
            _timer += Time.deltaTime;
            if (_timer >= HitTime)
            {
                _hit = false;
                // 만약 땅이 라면 Idle 애니메이션 트리거
                // 땅이 아니라면 Fall 애니메이션 트리거
                if (IsGrounded2D())
                {
                    _animator.SetTrigger("Land");
                }
                else
                {
                    _isFall = true;
                    _animator.SetTrigger("Fall");
                }
                _timer = 0f;
            }
        }

        if (_isFall)
        {
            if (IsGrounded2D())
            {
                _isFall = false;
                _animator.SetTrigger("Land");
            }
        }
    }

    protected override void OnExplosionImpact(int attackerViewId = 0)
    {
        // 로직 호출 가능 (예: 사운드, 이펙트, 애니메이션 등)\
        ApplyTemporaryDampingEffect();
        // 사운드
        SoundManager.Instance.PlayLocalRandomSound("PlayerDamage", transform, 1, 7, 0f, false, SoundType.SFX, true, 1f, 50f);
        //SoundManager.Instance.PlayLocalRandomSound("PlayerDamageVoice", transform, 1, 3, 0f, false, SoundType.SFX, true, 1f, 50f);
        // 애니메이션
        _animator.SetTrigger("Hit");
        _hit = true;
        _animator.ResetTrigger("Land");

        // 건파우더 낙출
        ReleaseGunPowder(attackerViewId);
    }
}
