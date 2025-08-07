using System;
using UnityEngine;

public class PlayerRecoilState : PlayerBaseState
{
    private float _recoilTimer;
    private float _yVelocitySpeed = 2f;

    [SerializeField] private float _damping = 5f;       // 감쇠 정도
    [SerializeField] private float _frequency = 10f;     // 반동 진동 빈도
    [SerializeField] private float _recoilOscillationScale = 0.2f;

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
        
        // 시간이 지나면 아이들 상태로
        if(_recoilTimer >= _owner.PlayerStat.RecoilTime)
        {
            if(_owner.PlayerStat.IsJumping)
            {
                _owner.RPC_SetAnimatorTrigger("Fall");
                _playerFSM.ChangeState<PlayerFallState>();
            }
            else
            {
                if (IsGrounded2D())
                {
                    _playerFSM.ChangeState<PlayerIdleState>();  
                }
                else
                {
                    _owner.RPC_SetAnimatorTrigger("Fall");
                    _playerFSM.ChangeState<PlayerFallState>();
                }
            }
            return;
        }
    }
}
