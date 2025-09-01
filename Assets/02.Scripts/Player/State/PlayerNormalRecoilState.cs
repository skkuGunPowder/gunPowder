using UnityEngine;

/// <summary>
/// 플레이어 일반 반동(리코일) 상태 클래스
/// 
/// 역할:
/// - 일반 폭탄/행동 후 짧은 반동 시간 유지
/// - 반동 시간 경과 후 점프/지면 상태에 따라 전이
/// 
/// 동작 방식:
/// 1. 진입 시 타이머 초기화 및 이동 속도 복원
/// 2. 매 프레임 타이머 갱신
/// 3. 반동 시간 종료 시 점프 여부와 지면 여부로 Idle/Fall 분기
/// </summary>
public class PlayerNormalRecoilState : PlayerBaseState
{
    private float _recoilTimer = 0f;

    /// <summary>
    /// 반동 상태 진입 초기화 (타이머/이동속도 복원)
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();
        _recoilTimer = 0f;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
    }

    /// <summary>
    /// 반동 상태 종료 시 정리
    /// </summary>
    public override void OnExit()
    {
        base.OnExit();
    }

    /// <summary>
    /// 반동 타이머 진행 및 상태 전이 처리
    /// </summary>
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
    /// 반동 시간 만료 여부 확인
    /// </summary>
    private bool IsRecoilTimeElapsed()
    {
        return _recoilTimer >= _owner.PlayerStat.NormalRecoilTime;
    }

    /// <summary>
    /// 반동 종료 후 Idle/Fall 상태 전이
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
            _playerFSM.ChangeState<PlayerIdleState>();
            return;
        }

        _owner.RPC_SetAnimatorTrigger("Fall");
        _playerFSM.ChangeState<PlayerFallState>();
    }
}
