using RobustFSM.Base;
using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    private float _keyReleaseTimer = 0f;
    private float _keyReleaseThreshold = 0.3f;

    // 코요테 타임 관련
    private float _coyoteTimer = 0f;
    private const float COYOTE_TIME = 0.1f;
    private bool _wasGroundedLastFrame = true;

    public override void OnEnter()
    {
        base.OnEnter();
        _keyReleaseTimer = 0f;
        _coyoteTimer = 0f;
        _wasGroundedLastFrame = IsGrounded2D();

        // 플레이어 상태
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.RunSpeed;
        _owner.PlayerStat.IsRunning = true;
        _owner.PlayerStat.IsJumping = false;

        // 애니메이션 재생
        _owner.RPC_SetAnimatorTrigger("Run");
    }

    public override void OnExit()
    {
        base.OnExit();
        _owner.RPC_ResetAnimatorTrigger("Run");
    }

    public override void MineUpdate()
    {
        base.MineUpdate();

        bool flowControl = RunMove();
        if (!flowControl)
        {
            return;
        }

        RunAttack();
    }

    private bool RunMove()
    {
        // 코요테 타임 및 바닥 체크
        bool isGrounded = IsGrounded2D();
        if (isGrounded)
        {
            _coyoteTimer = 0f;
            _wasGroundedLastFrame = true;
        }
        else
        {
            if (_wasGroundedLastFrame)
            {
                _coyoteTimer = 0f;
            }
            else
            {
                _coyoteTimer += Time.deltaTime;
            }

            if (_coyoteTimer >= COYOTE_TIME)
            {
                _owner.PlayerStat.IsFallingFromLedge = true;
                _owner.RPC_SetAnimatorTrigger("Fall");
                _playerFSM.ChangeState<PlayerFallState>();
                _wasGroundedLastFrame = false;
                return false;
            }

            _wasGroundedLastFrame = false;
        }

        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        if (InputHandler.GetKey(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == 1
        || InputHandler.GetKey(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == -1)
        {
            velocity.x = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.MyMoveSpeed;
            _owner.Rigidbody2D.linearVelocity = velocity;
        }
        else if (InputHandler.GetKey(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == -1
        || InputHandler.GetKey(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == 1)
        {
            _playerFSM.ChangeState<PlayerBreakState>();
        }
        else
        {
            _keyReleaseTimer += Time.deltaTime;
            velocity.x = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.MyMoveSpeed;
            _owner.Rigidbody2D.linearVelocity = velocity;
            if (_keyReleaseTimer >= _keyReleaseThreshold)
            {
                _playerFSM.ChangeState<PlayerIdleState>();
            }
        }

        return true;
    }

    private void RunAttack()
    {
        if (InputHandler.GetKeyDown(KeyCode.Z) && CanNormalBomb())
        {
            ThrowStraightNormalBomb();
            _playerFSM.ChangeState<PlayerRecoilState>();
            return;
        }
        if (InputHandler.GetKeyDown(KeyCode.X) && CanSpecialBomb())
        {
            ThrowStraightSpecialBomb();
            _playerFSM.ChangeState<PlayerRecoilState>();
            return;
        }
    }
} 