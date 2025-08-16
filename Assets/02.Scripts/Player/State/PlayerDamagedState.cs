using Photon.Pun;
using RobustFSM.Base;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PlayerDamagedState : PlayerBaseState
{
    private float _timer = 0f;
    private float _originalDrag;
    private Tween _dragTween;
    private float _startLinearDamping = 0.01f;
    private float _targetLinearDamping = 2.5f;

    public override void OnEnter()
    {
        base.OnEnter();

        _timer = 0f;
        // 애니메이션 재생
        _owner.RPC_SetAnimatorTrigger("Hit");
        
         // 무적
        _owner.gameObject.tag = "Immune";
        _owner.PlayerStat.IsImmune = true;
        
        // IsImmune을 네트워크로 동기화
        if (_owner.PhotonView.IsMine)
        {
            _owner.PhotonView.RPC(nameof(_owner.RPC_SetIsImmune), RpcTarget.All, true);
        }
        
        // 히트스탑에서 저장된 속도가 있다면 복원
        if (_owner.HasStoredVelocity)
        {
            _owner.RestoreVelocity();
        }
        
        // 맞은 횟수에 따른 추가 힘 적용
        ApplyDamageBasedForce();
        
        // Knockback 효과 적용
        // 플레이어의 건파우더가 50퍼 이하라면 (현재 피가 최대 피보다 클 수 있으므로 안전하게 처리)
        float currentHealthRatio = Mathf.Clamp01((float)_owner.PlayerStat.CurrentPlayerGunPowderCount / _owner.PlayerStat.InitGunpowderCount);
        ApplyKnockbackEffect(currentHealthRatio);

        // 히트 이펙트 활성화 및 방향 설정
        _owner.HitEffectPrefab.SetActive(true);
        //SetHitEffectDirection();
    }

    public override void OnExit()
    {
        base.OnExit();
        
        _owner.RPC_ResetAnimatorTrigger("Hit");
        _owner.RPC_ResetAnimatorTrigger("Walk");
        _owner.RPC_ResetAnimatorTrigger("Run");
        _owner.RPC_ResetAnimatorTrigger("Idle");
        _owner.RPC_ResetAnimatorTrigger("Dash");
        _owner.RPC_ResetAnimatorTrigger("Fall");
        _owner.StartCoroutine(HitEffectSetDeActiveCoroutine());

        // 무적 해제
        if(_owner.PhotonView.IsMine)
        {
            _owner.gameObject.tag = "Player";
        }
        else
        {
            _owner.gameObject.tag = "Enemy";
        }
        _owner.PlayerStat.IsImmune = false;
        
        // IsImmune을 네트워크로 동기화
        if (_owner.PhotonView.IsMine)
        {
            _owner.PhotonView.RPC(nameof(_owner.RPC_SetIsImmune), RpcTarget.All, false);
        }
        
        // 저장된 속도 상태 초기화
        _owner.ClearStoredVelocity();
        
        // Knockback 효과 정리
        CleanupKnockbackEffect();
    }

    private IEnumerator HitEffectSetDeActiveCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        _owner.HitEffectPrefab.SetActive(false);
    }

    public override void MineUpdate()
    {
        // 최소 피격 시간 보장
        _timer += Time.deltaTime;
        
        // 최소 피격 시간이 지나지 않았으면 상태 전환하지 않음
        if (_timer < _owner.PlayerStat.DamagedTime)
        {
            return;
        }
        
        // 최소 시간이 지난 후에 바닥에 닿으면 Idle 상태로 변환
        if (IsGrounded2D())
        {
            SyncStateChange<PlayerIdleState>();
            return;
        }
    }
    
    private void ApplyKnockbackEffect(float currentHealthRatio)
    {
        // 원래 drag 값 저장
        _originalDrag = _owner.Rigidbody2D.linearDamping;

        // 피의 비율에 따라 시작값 계산
        // 50%: 0.5, 0%: 0.01
        float startDamping;
        if (currentHealthRatio >= 0.5f)
        {
            startDamping = _originalDrag;
        }
        else
        {
            // 50%에서 0%까지 시작값이 0.5에서 0.01로 내려감
            float ratio = (0.5f - currentHealthRatio) / 0.5f; // 0~1 범위로 변환
            startDamping = Mathf.Lerp(0.5f, 0.01f, ratio);
        }

        _owner.Rigidbody2D.linearDamping = startDamping;
        
        // 시간이 지나면서 마찰을 점점 증가시켜 감속
        _dragTween?.Kill();
        _dragTween = DOTween.To(() => _owner.Rigidbody2D.linearDamping, x => 
        {
            _owner.Rigidbody2D.linearDamping = x;
        }, _targetLinearDamping, _owner.PlayerStat.DamagedTime)
        .SetEase(Ease.InOutBack);  
    }
    
    private void CleanupKnockbackEffect()
    {
        // Tween 정리
        _dragTween?.Kill();
        
        // 원래 drag 값으로 복원
        _owner.Rigidbody2D.linearDamping = _originalDrag;
    }
    
    /// <summary>
    /// 피 비율에 따라 추가 힘을 적용
    /// </summary>
    private void ApplyDamageBasedForce()
    {
        if (_owner.Rigidbody2D == null) return;
        
        // 현재 피 비율 계산 (0~1 범위, 최대 1.0으로 제한)
        float currentHealthRatio = Mathf.Clamp01((float)_owner.PlayerStat.CurrentPlayerGunPowderCount / _owner.PlayerStat.InitGunpowderCount);
        
        // 피 비율에 따른 추가 힘 계산
        // 피 100%일 때 추가 힘 0, 피 0%일 때 추가 힘 10
        float additionalForceMagnitude = (1.0f - currentHealthRatio) * 10.0f;
        
        // 현재 속도 방향으로 추가 힘 적용
        Vector2 currentVelocity = _owner.Rigidbody2D.linearVelocity;
        if (currentVelocity.magnitude > 0.1f) // 속도가 있을 때만 적용
        {
            Vector2 velocityDirection = currentVelocity.normalized;
            Vector2 additionalForce = velocityDirection * additionalForceMagnitude;
            
            // 추가 힘 적용
            _owner.Rigidbody2D.AddForce(additionalForce, ForceMode2D.Impulse);
            _owner.Rigidbody2D.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);
        }
    }
    
    /// <summary>
    /// 히트 이펙트의 방향을 플레이어 속도의 반대 방향으로 설정
    /// </summary>
    private void SetHitEffectDirection()
    {
        if (_owner.HitEffectPrefab == null || _owner.Rigidbody2D == null) return;
        
        Vector2 currentVelocity = _owner.Rigidbody2D.linearVelocity;
        
        // 속도가 있을 때만 방향 설정
        if (currentVelocity.magnitude > 0.1f)
        {
            // 속도의 반대 방향 계산
            Vector2 oppositeDirection = -currentVelocity.normalized;
            
            // Y축을 기준으로 회전 (파티클이 위쪽을 향하도록)
            float angle = Mathf.Atan2(oppositeDirection.y, oppositeDirection.x) * Mathf.Rad2Deg;
            
            // 히트 이펙트의 회전 설정
            _owner.HitEffectPrefab.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // 속도가 없으면 기본 방향 (위쪽)으로 설정
            _owner.HitEffectPrefab.transform.rotation = Quaternion.Euler(0, 0, 90f);
        }
    }
}
