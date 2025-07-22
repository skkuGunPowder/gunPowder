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
    private const float COYOTE_TIME = 0.15f;
    private bool _wasGroundedLastFrame = true;

    public override void OnEnter()
    {
        base.OnEnter();

        // 더블탭 변수 초기화
        _lastKeyPressTime = 0f;
        _isKeyPressed = false;
        _keyReleaseTimer = 0f;

        // 코요테 타임 초기화
        _coyoteTimer = 0f;
        _wasGroundedLastFrame = IsGrounded2D();

        // 플레이어 상태
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
        _owner.PlayerStat.IsRunning = false;

        // 애니메이션 재생
        _owner.SetAnimatorTrigger("Walk");
    }
    
    public override void OnExit()
    {
        base.OnExit();
        _owner.ResetAnimatorTrigger("Walk");
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
            _owner.SetAnimatorTrigger("Fall");
            _playerFSM.ChangeState<PlayerJumpState>();
            return false;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            _owner.SetFacingDirection(1);

            // 키 입력 감지
            if (!_isKeyPressed)
            {
                _isKeyPressed = true;
                float currentTime = Time.time;

                // 더블탭 체크 (같은 방향이고, 시간 간격이 짧을 때)
                if (_owner.PlayerStat.FacingDirection == 1 && (currentTime - _lastKeyPressTime) <= _owner.PlayerStat.DoubleTapTime)
                {
                    Debug.Log("WalkState: 오른쪽 더블탭 감지 - DashState로 전환");
                    _playerFSM.ChangeState<PlayerDashState>();
                    return false;
                }

                _lastKeyPressTime = currentTime;
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            _owner.SetFacingDirection(-1);

            // 키 입력 감지
            if (!_isKeyPressed)
            {
                _isKeyPressed = true;
                float currentTime = Time.time;

                // 더블탭 체크 (같은 방향이고, 시간 간격이 짧을 때)
                if (_owner.PlayerStat.FacingDirection == -1 && (currentTime - _lastKeyPressTime) <= _owner.PlayerStat.DoubleTapTime)
                {
                    Debug.Log("WalkState: 왼쪽 더블탭 감지 - DashState로 전환");
                    _playerFSM.ChangeState<PlayerDashState>();
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
                Debug.Log("WalkState: 키를 떼어서 IdleState로 전환");
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
        if (Input.GetKeyDown(KeyCode.Z) && CanNormalBomb())
        {
            ThrowNormalBomb();
            _playerFSM.ChangeState<PlayerNormalRecoilState>();
            return;
        }
        if (Input.GetKeyDown(KeyCode.X) && CanSpecialBomb())
        {
            ThrowSpecialBomb();
            _playerFSM.ChangeState<PlayerNormalRecoilState>();
            return;
        }
    }
}
