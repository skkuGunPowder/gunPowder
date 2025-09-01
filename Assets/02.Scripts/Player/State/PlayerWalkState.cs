using UnityEngine;

/// <summary>
/// 플레이어 걷기 상태 클래스
/// 
/// 역할:
/// - 걷기 중 좌우 입력에 따른 이동/대시 더블탭 감지
/// - 코요테 타임 기반 낙하 전환, 키 해제 기반 Idle 전환
/// - 걷기 중 폭탄 공격 처리(투척/설치 + NormalRecoil 전환)
/// 
/// 동작 방식:
/// 1. 진입 시 더블탭/코요테/방향/타이머 초기화 및 애니메이션 설정
/// 2. 매 프레임 코요테/지면 체크 → 낙하 전환 판단
/// 3. 좌우 입력 우선순위(right 우선)로 이동/더블탭 대시 전환 처리
/// 4. 키 해제 지속 시 Idle 전환, 이동은 Rigidbody2D로 적용
/// </summary>
public class PlayerWalkState : PlayerBaseState
{
    // 대쉬 타이머
    private float _timer = 0f;
    

    // 더블탭 감지용 변수들 (방향별로 독립적으로 관리)
    private float _lastRightKeyDownTime = -999f;
    private float _lastLeftKeyDownTime = -999f;
    private bool _isKeyPressed = false;
    private float _keyReleaseTimer = 0f;
    private const float KEY_RELEASE_THRESHOLD = 0.1f; // 키를 떼고 이 시간 이내에 다시 누르면 더블탭으로 인식

    // 코요테 타임 관련
    private float _coyoteTimer = 0f;
    private const float COYOTE_TIME = 0.1f;
    private bool _wasGroundedLastFrame = true;
    
    // 방향 변경 감지용
    private int _lastFacingDirection = 0;

    /// <summary>
    /// 걷기 상태 진입 초기화 (더블탭/코요테/방향/애니메이션)
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();

        // 더블탭 변수 초기화
        _lastRightKeyDownTime = -999f;
        _lastLeftKeyDownTime = -999f;
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

        // Seed: 착지 직전/상태 전환 직전 입력을 이어받아 같은 방향 더블탭만 유효하도록 저장
        if (_owner.LastDashTapTimeRight > 0f)
        {
            _lastRightKeyDownTime = _owner.LastDashTapTimeRight;
        }
        if (_owner.LastDashTapTimeLeft > 0f)
        {
            _lastLeftKeyDownTime = _owner.LastDashTapTimeLeft;
        }
    }
    
    /// <summary>
    /// 걷기 상태 종료 정리 (애니메이션 리셋)
    /// </summary>
    public override void OnExit()
    {
        base.OnExit();
        _owner.RPC_ResetAnimatorTrigger("Walk");
    }

    /// <summary>
    /// 걷기 메인 업데이트: 코요테/지면 → 입력/대시감지 → 이동/공격
    /// </summary>
    public override void MineUpdate()
    {
        base.MineUpdate();

        if (!HandleCoyoteAndGroundTransition())
        {
            return;
        }

        if (!HandleWalkInputsAndDashDetection())
        {
            return;
        }

        ApplyWalkVelocity();
        WalkAttack();
    }

    /// <summary>
    /// 코요테/지면 체크 및 낙하 전환 판단
    /// </summary>
    private bool HandleCoyoteAndGroundTransition()
    {
        _timer += Time.deltaTime;
        _keyReleaseTimer += Time.deltaTime;

        bool isGrounded = IsGrounded2D();
        if (isGrounded)
        {
            _coyoteTimer = 0f;
            _wasGroundedLastFrame = true;
            return true;
        }

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
        return true;
    }

    /// <summary>
    /// 입력 처리 및 대시 더블탭 감지 (Right 우선)
    /// </summary>
    private bool HandleWalkInputsAndDashDetection()
    {
        // 반대 방향 새 입력 발생 시, 기존 홀드 입력을 무시하고 새 입력을 탭으로 인식
        if (InputHandler.GetKeyDown(KeyCode.RightArrow))
        {
            _isKeyPressed = false;
            _lastLeftKeyDownTime = -999f;
        }
        else if (InputHandler.GetKeyDown(KeyCode.LeftArrow))
        {
            _isKeyPressed = false;
            _lastRightKeyDownTime = -999f;
        }

        if (InputHandler.GetKey(KeyCode.RightArrow))
        {
            if (_lastFacingDirection != 1)
            {
                _owner.RPC_SetFacingDirection(1);
                _lastFacingDirection = 1;
                _lastLeftKeyDownTime = -999f;
            }

            if (!_isKeyPressed)
            {
                _isKeyPressed = true;
                float currentTime = Time.time;

                float rightSeed = Mathf.Max(_lastRightKeyDownTime, _owner.LastDashTapTimeRight);
                if (_owner.PlayerStat.FacingDirection == 1 && (currentTime - rightSeed) <= _owner.PlayerStat.DoubleTapTime)
                {
                    _playerFSM.ChangeState<PlayerDashState>();
                    _owner.LastDashTapTimeRight = -999f;
                    return false;
                }

                _lastRightKeyDownTime = currentTime;
            }
        }
        else if (InputHandler.GetKey(KeyCode.LeftArrow))
        {
            if (_lastFacingDirection != -1)
            {
                _owner.RPC_SetFacingDirection(-1);
                _lastFacingDirection = -1;
                _lastRightKeyDownTime = -999f;
            }

            if (!_isKeyPressed)
            {
                _isKeyPressed = true;
                float currentTime = Time.time;

                float leftSeed = Mathf.Max(_lastLeftKeyDownTime, _owner.LastDashTapTimeLeft);
                if (_owner.PlayerStat.FacingDirection == -1 && (currentTime - leftSeed) <= _owner.PlayerStat.DoubleTapTime)
                {
                    _playerFSM.ChangeState<PlayerDashState>();
                    _owner.LastDashTapTimeLeft = -999f;
                    return false;
                }

                _lastLeftKeyDownTime = currentTime;
            }
        }
        else
        {
            if (_isKeyPressed)
            {
                _isKeyPressed = false;
                _keyReleaseTimer = 0f;
            }

            if (_keyReleaseTimer >= KEY_RELEASE_THRESHOLD)
            {
                _playerFSM.ChangeState<PlayerIdleState>();
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 걷기 수평 속도 적용
    /// </summary>
    private void ApplyWalkVelocity()
    {
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        velocity.x = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.MyMoveSpeed;
        _owner.Rigidbody2D.linearVelocity = velocity;
    }

    /// <summary>
    /// 걷기 중 공격 처리 (투척/설치 후 NormalRecoil 전환)
    /// </summary>
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
