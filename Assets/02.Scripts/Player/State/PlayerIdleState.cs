using RobustFSM.Base;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public override void OnEnter()
    {
        base.OnEnter();
        // 플레이어 상태
        _owner.PlayerStat.IsRunning = false;
        _owner.PlayerStat.IsJumping = false;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;  // 기본 이동속도
        _owner.PlayerStat.JumpCount = 0;
        _owner.PlayerStat.ResetJumpDashCount();

        // 애니메이션 재생
        // _owner.MyAnimator.SetTrigger("Idle");
    }
    public override void OnExit()
    {
        base.OnExit();
    }

    /// <summary>
    /// 실제 행동 로직
    /// </summary>
    public override void Update()
    {
        base.Update();

        // 이동키를 받으면 걷기 상태로 전환
        if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)
        || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
         {
            _owner.SetFacingDirection(Input.GetKey(KeyCode.LeftArrow) ? -1 : 1);
            _playerFSM.ChangeState<PlayerWalkState>();
         }
    }
}
