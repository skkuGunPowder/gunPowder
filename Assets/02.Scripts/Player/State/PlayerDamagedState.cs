using Photon.Pun;
using RobustFSM.Base;
using UnityEngine;
using DG.Tweening;

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
        
        // 히트스탑에서 저장된 속도가 있다면 복원
        if (_owner.HasStoredVelocity)
        {
            _owner.RestoreVelocity();
        }
        
        // Knockback 효과 적용
        // 플레이어의 건파우더가 50퍼 이하라면 (현재 피가 최대 피보다 클 수 있으므로 안전하게 처리)
        float currentHealthRatio = Mathf.Clamp01((float)_owner.PlayerStat.CurrentPlayerGunPowderCount / _owner.PlayerStat.InitGunpowderCount);
        ApplyKnockbackEffect(currentHealthRatio);
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
        
        // 저장된 속도 상태 초기화
        _owner.ClearStoredVelocity();
        
        // Knockback 효과 정리
        CleanupKnockbackEffect();
    }

    public override void MineUpdate()
    {
        // 피격 시간 로직
        // 피격 시간이 끝나면 피격 상태 종료
        _timer += Time.deltaTime;

        if (_timer >= _owner.PlayerStat.DamagedTime)
        {
            /*
            if (IsGrounded2D())
            {
                _playerFSM.ChangeState<PlayerIdleState>();
                return;
            }
            else
            {
                _owner.PlayerStat.IsFallingFromLedge = true;
                _owner.RPC_SetAnimatorTrigger("Fall");
                _playerFSM.ChangeState<PlayerJumpState>();
                return;
            }*/
            
            _playerFSM.ChangeState<PlayerIdleState>();
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
}
