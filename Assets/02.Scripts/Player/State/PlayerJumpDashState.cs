using UnityEngine;
using RobustFSM.Base;

public class PlayerJumpDashState : PlayerBaseState
{
    private float _dashTimer = 0f;
    private float _yVelocity = 0f;
    private float _xVelocity = 0f;
    private float _gravity = -40f;

    public override void OnEnter()
    {
        base.OnEnter();
        // 플레이어 상태
        _owner.PlayerStat.IsRunning = false;
        _owner.PlayerStat.IsJumping = true;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.DashSpeed;
        _owner.PlayerStat.IncrementJumpDashCount();
        
        _yVelocity = 0f;
        _xVelocity = 0f;
        _dashTimer = 0f;

        // 애니메이션 재생
        // _owner.MyAnimator.SetTrigger("Dash");
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
        
        // 대쉬 시간 종료 후 점프 상태와 같이 움직임
        if(_dashTimer >= _owner.PlayerStat.DashTime)
        {
            _playerFSM.ChangeState<PlayerJumpState>();
        }
        else
        {
            // 대쉬 이동후 낙하
            _dashTimer += Time.deltaTime;
            _owner.CharacterController.Move(new Vector3(_owner.PlayerStat.FacingDirection, 0, 0) 
                                            * _owner.PlayerStat.MyMoveSpeed * Time.deltaTime);
        }
    }
}
