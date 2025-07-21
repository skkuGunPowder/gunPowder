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
        _owner.SetAnimatorTrigger("Hit");
    }

    public override void OnExit()
    {
        base.OnExit();
        _owner.ResetAnimatorTrigger("Hit");
    }

    public override void Update()
    {
        // 피격 시간 로직
        // 피격 시간이 끝나면 피격 상태 종료
        _timer += Time.deltaTime;

        if(_timer >= _owner.PlayerStat.DamagedTime)
        {
            if(IsGrounded())
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
