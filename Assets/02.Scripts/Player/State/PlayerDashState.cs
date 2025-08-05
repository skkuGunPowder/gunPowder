using RobustFSM.Base;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private float _dashTimer = 0f;
    private float _originalGravityScale;

    public override void OnEnter()
    {
        base.OnEnter();

        _dashTimer = 0f;

        // 플레이어 상태
        _owner.PlayerStat.IsRunning = true;
        _owner.PlayerStat.IsJumping = false;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.DashSpeed;
        _originalGravityScale = _owner.Rigidbody2D.gravityScale;
        _owner.Rigidbody2D.gravityScale = 0f;

        // 애니메이션 재생
        _owner.RPC_SetAnimatorTrigger("Dash");
    }
    public override void OnExit()
    {
        base.OnExit();
        _owner.Rigidbody2D.gravityScale = _originalGravityScale;
        _owner.RPC_ResetAnimatorTrigger("Dash");
    }

    /// <summary>
    /// 실제 행동 로직
    /// </summary>
    public override void MineUpdate()
    {
        _dashTimer += Time.deltaTime;

        // 1. 대시 이동(관성)
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        velocity.x = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.MyMoveSpeed;
        velocity.y = 0;
        _owner.Rigidbody2D.linearVelocity = velocity;

        int dir = _owner.PlayerStat.FacingDirection;
        // 2. 대시 중 반대 방향 키 입력 체크 → BreakState로 전환
        if(InputHandler.GetKeyDown(KeyCode.RightArrow) && dir == -1 || InputHandler.GetKeyDown(KeyCode.LeftArrow) && dir == 1)
        {
            if(!IsGrounded2D())
            {
                return;
            }
            _playerFSM.ChangeState<PlayerBreakState>();
            return;
        }

        // 3. 대시 시간 종료 후 상태 전이
        if(_dashTimer >= _owner.PlayerStat.DashTime)
        {
            // 같은 방향 키 누르고 있음 → Run
            if ((_owner.PlayerStat.FacingDirection == 1 && InputHandler.GetKey(KeyCode.RightArrow)) 
            || (_owner.PlayerStat.FacingDirection == -1 && InputHandler.GetKey(KeyCode.LeftArrow)))
            {
                _playerFSM.ChangeState<PlayerRunState>();
                return;
            }
            // 아무 키도 안 누름 → Idle
            else
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
}
