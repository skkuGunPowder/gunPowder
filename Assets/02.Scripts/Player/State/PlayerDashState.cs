using RobustFSM.Base;
using UnityEngine;

/// <summary>
/// 플레이어 대시 상태 클래스
/// 
/// 역할:
/// - 대시 중 수평 이동 고정 및 중력 비활성
/// - 반대 방향 입력 시 지상에서만 브레이크 상태 전환
/// - 대시 종료 후 입력/바닥 상태에 따른 상태 전환
/// 
/// 동작 방식:
/// 1. 진입 시 속도/중력/애니메이션 설정
/// 2. 매 프레임 수평 속도 고정(관성), 반대 입력 체크
/// 3. 대시 시간 경과 시 Run/Idle/Fall로 분기
/// </summary>
public class PlayerDashState : PlayerBaseState
{
    private float _dashTimer = 0f;
    private float _originalGravityScale;

    public override void OnEnter()
    {
        base.OnEnter();
        _dashTimer = 0f;

        // 플레이어 상태 설정
        _owner.PlayerStat.IsRunning = true;
        _owner.PlayerStat.IsJumping = false;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.DashSpeed;

        // 중력 비활성화
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

        ApplyDashMovement();

        if (TryBreakFromOppositeInput())
        {
            return;
        }

        HandleDashEndTransition();
    }


    private void ApplyDashMovement()
    {
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        velocity.x = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.MyMoveSpeed;
        velocity.y = 0;
        _owner.Rigidbody2D.linearVelocity = velocity;
    }

    private bool TryBreakFromOppositeInput()
    {
        int dir = _owner.PlayerStat.FacingDirection;

        bool pressedOpposite = (InputHandler.GetKeyDown(KeyCode.RightArrow) && dir == -1) ||
                               (InputHandler.GetKeyDown(KeyCode.LeftArrow) && dir == 1);

        if (!pressedOpposite)
        {
            return false;
        }

        if (!IsGrounded2D())
        {
            return true; // 공중이면 상태 전환 없이 종료
        }

        _playerFSM.ChangeState<PlayerBreakState>();
        return true;
    }

    private void HandleDashEndTransition()
    {
        if (_dashTimer < _owner.PlayerStat.DashTime)
        {
            return;
        }

        bool sameDirectionKeyHeld = (_owner.PlayerStat.FacingDirection == 1 && InputHandler.GetKey(KeyCode.RightArrow)) ||
                                    (_owner.PlayerStat.FacingDirection == -1 && InputHandler.GetKey(KeyCode.LeftArrow));

        if (sameDirectionKeyHeld)
        {
            _playerFSM.ChangeState<PlayerRunState>();
            return;
        }

        if (IsGrounded2D())
        {
            _playerFSM.ChangeState<PlayerIdleState>();
            return;
        }

        _owner.PlayerStat.IsFallingFromLedge = true;
        _owner.RPC_SetAnimatorTrigger("Fall");
        _playerFSM.ChangeState<PlayerFallState>();
    }
}
