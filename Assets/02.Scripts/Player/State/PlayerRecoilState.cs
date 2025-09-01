using System;
using UnityEngine;

/// <summary>
/// 플레이어 강한 반동(리코일) 상태 클래스
/// 
/// 역할:
/// - 강한 공격 후 짧은 반동 시간 유지
/// - 반동 시간 종료 시 점프/지면 상태에 따라 전이
/// 
/// 동작 방식:
/// 1. 진입 시 타이머 초기화 및 이동 속도/러닝 플래그 정리
/// 2. 매 프레임 타이머 갱신
/// 3. 반동 시간 경과 시 Idle/Fall로 분기 (점프 여부 우선)
/// </summary>
public class PlayerRecoilState : PlayerBaseState
{
    private float _recoilTimer;

    public override void OnEnter()
    {
        base.OnEnter();
        _recoilTimer = 0f;
        _owner.PlayerStat.IsRunning = false;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
    }

    public override void OnExit()
    {
        base.OnExit();
        _owner.RPC_ResetAnimatorTrigger("JumpAttack");
        _owner.RPC_ResetAnimatorTrigger("JumpStrongAttack");
    }

    public override void MineUpdate()
    {
        _recoilTimer += Time.deltaTime;

        if (!IsRecoilTimeElapsed())
        {
            return;
        }

        TransitionAfterRecoil();
    }

    /// <summary>
    /// 반동 시간 경과 여부 확인
    /// </summary>
    /// <returns></returns>
    private bool IsRecoilTimeElapsed()
    {
        return _recoilTimer >= _owner.PlayerStat.RecoilTime;
    }

    /// <summary>
    /// 반동 종료 후 점프/지면 상태에 따라 전이
    /// </summary>
    private void TransitionAfterRecoil()
    {
        if (_owner.PlayerStat.IsJumping)
        {
            _owner.RPC_SetAnimatorTrigger("Fall");
            _playerFSM.ChangeState<PlayerFallState>();
            return;
        }

        if (IsGrounded2D())
        {
            _playerFSM.SyncStateChange<PlayerIdleState>();
            return;
        }

        _owner.RPC_SetAnimatorTrigger("Fall");
        _playerFSM.ChangeState<PlayerFallState>();
    }
}
