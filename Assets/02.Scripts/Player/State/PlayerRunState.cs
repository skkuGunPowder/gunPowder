using RobustFSM.Base;
using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    private float _keyReleaseTimer = 0f;
    private float _keyReleaseThreshold = 0.1f;

    // 코요테 타임 관련
    private float _coyoteTimer = 0f;
    private const float COYOTE_TIME = 0.15f;
    private bool _wasGroundedLastFrame = true;

    public override void OnEnter()
    {
        base.OnEnter();
        _keyReleaseTimer = 0f;
        _coyoteTimer = 0f;
        _wasGroundedLastFrame = IsGrounded();

        // 플레이어 상태
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.RunSpeed;
        _owner.PlayerStat.IsRunning = true;

        // 애니메이션 재생
        // _owner.MyAnimator.SetTrigger("Run");
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void Update()
    {
        base.Update();

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
        bool isGrounded = IsGrounded();
        if (isGrounded)
        {
            _coyoteTimer = 0f;
        }
        else
        {
            _coyoteTimer += Time.deltaTime;
        }

        // 바닥에서 떨어진 순간(이전 프레임엔 있었고, 이번 프레임엔 없음)
        if (!isGrounded)
        {
            _owner.PlayerStat.IsFallingFromLedge = true;
            _playerFSM.ChangeState<PlayerJumpState>();
            return false;
        }

        if (Input.GetKey(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == 1
        || Input.GetKey(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == -1)
        {
            _owner.CharacterController.Move(new Vector3(_owner.PlayerStat.FacingDirection * _owner.PlayerStat.MyMoveSpeed * Time.deltaTime,
             0, 0));
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == -1
        || Input.GetKeyUp(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == 1)
        {
            _playerFSM.ChangeState<PlayerBreakState>();
        }
        else
        {
            _keyReleaseTimer += Time.deltaTime;
            if (_keyReleaseTimer >= _keyReleaseThreshold)
            {
                _playerFSM.ChangeState<PlayerIdleState>();
            }
        }

        return true;
    }

    private void RunAttack()
    {
        if (Input.GetKeyDown(KeyCode.Z) && CanNormalBomb())
        {
            _owner.NormalBomb.ThrowBombStraight(_owner.GetBombSpawnPoint());
            SetLastNormalBombTime();
        }
        if (Input.GetKeyDown(KeyCode.X) && CanSpecialBomb())
        {
            _owner.SpecialBomb.ThrowBombStraight(_owner.GetBombSpawnPoint());
            SetLastSpecialBombTime();
        }
    }
} 