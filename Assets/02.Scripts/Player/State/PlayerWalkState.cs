using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    // 대쉬 타이머
    private float _timer = 0f;
    

    // 더블탭 감지용 변수들
    private float _lastKeyPressTime = 0f;
    private bool _isKeyPressed = false;
    private float _keyReleaseTimer = 0f;
    private const float KEY_RELEASE_THRESHOLD = 0.1f; // 키를 떼고 이 시간 이내에 다시 누르면 더블탭으로 인식

    // 코요테 타임 관련
    private float _coyoteTimer = 0f;
    private const float COYOTE_TIME = 0.1f;
    private bool _wasGroundedLastFrame = true;
    
    // 방향 변경 감지용
    private int _lastFacingDirection = 0;

    public override void OnEnter()
    {
        base.OnEnter();

        // 더블탭 변수 초기화
        _lastKeyPressTime = 0f;
        _isKeyPressed = false;
        _keyReleaseTimer = 0f;
        _lastFacingDirection = _owner.PlayerStat.FacingDirection;

        // 코요테 타임 초기화
        _coyoteTimer = 0f;
        _wasGroundedLastFrame = IsGrounded2D();

        // 플레이어 상태
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
        _owner.PlayerStat.IsRunning = false;
        _owner.PlayerStat.IsJumping = false;

        // 애니메이션 재생
        _owner.RPC_ResetAnimatorTrigger("Idle");
        _owner.RPC_SetAnimatorTrigger("Walk");

        // Seed double-tap timing if a tap occurred just before landing (airborne)
        // so that the next keydown within DoubleTapTime triggers dash immediately.
        if (_owner.PlayerStat.FacingDirection == 1)
        {
            if (_owner.LastDashTapTimeRight > 0f)
            {
                _lastKeyPressTime = _owner.LastDashTapTimeRight;
            }
        }
        else if (_owner.PlayerStat.FacingDirection == -1)
        {
            if (_owner.LastDashTapTimeLeft > 0f)
            {
                _lastKeyPressTime = _owner.LastDashTapTimeLeft;
            }
        }
    }
    
    public override void OnExit()
    {
        base.OnExit();
        _owner.RPC_ResetAnimatorTrigger("Walk");
    }

    /// <summary>
    /// 실제 행동 로직
    /// </summary>
    public override void MineUpdate()
    {
        base.MineUpdate();

        // 이동 로직
        bool flowControl = WalkMove();
        if (!flowControl)
        {
            return;
        }

        // 공격 로직
        WalkAttack();
    }

    private bool WalkMove()
    {
        _timer += Time.deltaTime;
        _keyReleaseTimer += Time.deltaTime;

        // 코요테 타임 및 바닥 체크
        bool isGrounded = IsGrounded2D();

        if (isGrounded)
        {
            // 바닥에 있는 동안 타이머 초기화
            _coyoteTimer = 0f;
            _wasGroundedLastFrame = true;
        }
        else
        {
            // 바닥을 벗어난 첫 프레임이면 타이머 초기화만 하고 유지
            if (_wasGroundedLastFrame)
            {
                _coyoteTimer = 0f;
            }
            else
            {
                _coyoteTimer += Time.deltaTime;
            }

            // 코요테 타임이 끝났을 때만 낙하 상태로 전환
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

        if (InputHandler.GetKey(KeyCode.RightArrow))
        {
            // 방향이 바뀔 때만 RPC 호출
            if (_lastFacingDirection != 1)
            {
                _owner.RPC_SetFacingDirection(1);
                _lastFacingDirection = 1;
                // 방향이 바뀌면 현재 시간으로 설정하여 다음 키 입력에서 더블탭 감지 가능하도록 함
                _lastKeyPressTime = Time.time;
            }

            // 키 입력 감지
            if (!_isKeyPressed)
            {
                _isKeyPressed = true;
                float currentTime = Time.time;

                // 더블탭 체크 (같은 방향이고, 시간 간격이 짧을 때)
                float rightSeed = Mathf.Max(_lastKeyPressTime, _owner.LastDashTapTimeRight);
                if (_owner.PlayerStat.FacingDirection == 1 && (currentTime - rightSeed) <= _owner.PlayerStat.DoubleTapTime)
                {
                    _playerFSM.ChangeState<PlayerDashState>();
                    // clear seed to avoid stale reuse
                    _owner.LastDashTapTimeRight = -999f;
                    return false;
                }

                _lastKeyPressTime = currentTime;
            }
        }
        else if (InputHandler.GetKey(KeyCode.LeftArrow))
        {
            // 방향이 바뀐 때만 RPC 호출
            if (_lastFacingDirection != -1)
            {
                _owner.RPC_SetFacingDirection(-1);
                _lastFacingDirection = -1;
                // 방향이 바뀌면 현재 시간으로 설정하여 다음 키 입력에서 더블탭 감지 가능하도록 함
                _lastKeyPressTime = Time.time;
            }

            // 키 입력 감지
            if (!_isKeyPressed)
            {
                _isKeyPressed = true;
                float currentTime = Time.time;

                // 더블탭 체크 (같은 방향이고, 시간 간격이 짧을 때)
                float leftSeed = Mathf.Max(_lastKeyPressTime, _owner.LastDashTapTimeLeft);
                if (_owner.PlayerStat.FacingDirection == -1 && (currentTime - leftSeed) <= _owner.PlayerStat.DoubleTapTime)
                {
                    _playerFSM.ChangeState<PlayerDashState>();
                    // clear seed to avoid stale reuse
                    _owner.LastDashTapTimeLeft = -999f;
                    return false;
                }

                _lastKeyPressTime = currentTime;
            }
        }
        else
        {
            // 키를 떼었을 때
            if (_isKeyPressed)
            {
                _isKeyPressed = false;
                _keyReleaseTimer = 0f;
            }

            // 키를 떼고 일정 시간이 지나면 Idle로 전환
            if (_keyReleaseTimer >= KEY_RELEASE_THRESHOLD)
            {
                _playerFSM.ChangeState<PlayerIdleState>();
                return false;
            }
        }

        // 실제 이동 처리 (Rigidbody2D 사용)
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        velocity.x = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.MyMoveSpeed;
        _owner.Rigidbody2D.linearVelocity = velocity;

        return true;
    }

    private void WalkAttack()
    {
        if (InputHandler.GetKeyDown(KeyCode.Z) && CanNormalBomb())
        {
            ThrowNormalBomb();
            _playerFSM.ChangeState<PlayerNormalRecoilState>();
            return;
        }
        if (InputHandler.GetKeyDown(KeyCode.X) && CanSpecialBomb())
        {
            ThrowSpecialBomb();
            _playerFSM.ChangeState<PlayerNormalRecoilState>();
            return;
        }
    }
}
