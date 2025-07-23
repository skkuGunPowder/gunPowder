using Photon.Pun;
using RobustFSM.Base;
using UnityEngine;

public class PlayerDamagedState : PlayerBaseState
{
    private float _timer = 0f;

    public override void OnEnter()
    {
        base.OnEnter();

        _timer = 0f;
        // 애니메이션 재생
        _owner.RPC_SetAnimatorTrigger("Hit");
    }

    public override void OnExit()
    {
        base.OnExit();
        _owner.RPC_ResetAnimatorTrigger("Hit");
        _owner.RPC_ResetAnimatorTrigger("Walk");
        _owner.RPC_ResetAnimatorTrigger("Run");
        _owner.RPC_ResetAnimatorTrigger("Idle");
        _owner.RPC_ResetAnimatorTrigger("Dash");
    }

    public override void MineUpdate()
    {
        // 피격 시간 로직
        // 피격 시간이 끝나면 피격 상태 종료
        _timer += Time.deltaTime;

        if(_timer >= _owner.PlayerStat.DamagedTime)
        {
            if(IsGrounded2D())
            {
                _playerFSM.ChangeState<PlayerIdleState>();
                return;
            }
            else
            {
                _owner.PlayerStat.IsFallingFromLedge = true;
                _owner.SetAnimatorTrigger("Fall");
                _playerFSM.ChangeState<PlayerJumpState>();
                return;
            }
        }
    }
}
