using System;
using Photon.Pun;
using RobustFSM.Base;
using UnityEngine;

/// <summary>
/// 플레이어 낙하 상태 클래스
/// 
/// 역할:
/// - 플레이어가 공중에서 떨어지는 동안의 처리
/// - 공중에서의 좌우 이동 및 러닝 상태 관리
/// - 착지 감지 및 점프 전환 처리
/// - 공중 공격 (폭탄 설치/투척) 처리
/// - 공중 대시 (폭탄 대시, 점프 대시) 처리
/// 
/// 동작 방식:
/// 1. 낙하 중 좌우 이동 처리 (러닝/일반 이동)
/// 2. Y축 속도 제한으로 최대 낙하 속도 제어
/// 3. 착지 감지 및 상태 전환
/// 4. 상승 시 점프 상태로 전환
/// 5. 공중 공격 및 대시 입력 처리
/// </summary>
public class PlayerFallState : PlayerBaseState
{
    // 물리 관련 상수
    private const float MAX_FALL_SPEED = -20f;                  // 최대 낙하 속도 (음수)
    private const float JUMP_DETECTION_THRESHOLD = 5f;          // 점프 상태 전환 Y 속도 임계값
    private const float VELOCITY_LERP_SPEED = 3f;               // 속도 보간 속도 (높을수록 반응 빠름)
    
    // 공중 제어 관련 상수
    private const float AIR_CONTROL_MULTIPLIER = 1.5f;          // 공중 이동 속도 배율 (지상 대비)
    private const float AIR_RUNNING_MULTIPLIER = 1.5f;          // 공중 러닝 속도 배율 (지상 러닝 대비)
    
    // 타이머 관련 상수
    private const float KEY_RELEASE_THRESHOLD = 0.3f;           // 키 릴리즈 임계값 (초)
    private const float EXPLOSION_OVERRIDE_DURATION = 0.3f;     // 폭발 후 X축 속도 덮어쓰기 차단 시간 (초)
    private const float LANDING_CONFIRM_TIME = 0.1f;            // 착지 확인 시간 (초)
    
    // 방향 상수
    private const int DIRECTION_LEFT = -1;                      // 왼쪽 방향
    private const int DIRECTION_RIGHT = 1;                      // 오른쪽 방향
    private const int DIRECTION_NONE = 0;                       // 방향 없음
    
    // 이동 관련 변수들
    private float _horizontalVelocity = 0f;                     // 수평 이동 속도
    private float _fallTimer = 0f;                              // 낙하 시간 타이머
    private float _originalGravityScale;                        // 원본 중력 스케일
    
    // 입력 관련 변수들
    private float _keyReleaseTimer = 0f;                        // 키 릴리즈 타이머
    private float _lastLeftTapTime = 0f;                        // 마지막 왼쪽 키 탭 시간
    private float _lastRightTapTime = 0f;                       // 마지막 오른쪽 키 탭 시간
    private int _lastFacingDirection = DIRECTION_NONE;          // 마지막 바라본 방향
    
    // 폭발 효과 관련 변수들
    private float _explosionOverrideTimer = 0f;                 // 폭발 후 X축 속도 덮어쓰기 차단 타이머
    
    // 착지 감지 관련 변수들
    private bool _wasGroundedLastFrame = false;                 // 이전 프레임에 착지했는지 여부
    private float _landingTimer = 0f;                           // 착지 확인 타이머

    /// <summary>
    /// 낙하 상태 진입 시 초기화
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();

        // 애니메이션 설정
        SetupFallAnimation();

        // 점프 카운트 관리
        HandleJumpCountIncrement();

        _owner.PlayerStat.IsJumping = true;

        // 공중 전환 이벤트 발행
        if (_owner.PhotonView.IsMine)
        {
            PlayerEventManager.Instance.GetEvents(_owner.ActorNumber).InvokeOnAirborne();
        }

        // 착지 감지 초기화
        InitializeLandingDetection();

        // 타이머 및 설정 초기화
        InitializeFallState();
    }

    /// <summary>
    /// 낙하 상태 종료 시 정리 작업
    /// </summary>
    public override void OnExit()
    {
        _owner.RPC_ResetAnimatorTrigger("Fall");
        
        _owner.Rigidbody2D.gravityScale = _originalGravityScale;
        _owner.PlayerStat.IsJumping = false;

        base.OnExit();
    }

    /// <summary>
    /// 낙하 상태의 메인 업데이트 로직
    /// </summary>
    public override void MineUpdate()
    {
        // 타이머 업데이트
        UpdateTimers();

        // Y축 속도 제한 적용
        LimitYVelocity();

        // 상승 시 점프 상태로 전환
        if (CheckForJumpTransition())
        {
            return;
        }

        // 착지 감지 및 상태 전환
        if (CheckForLandingTransition())
        {
            return;
        }

        // 이동 처리
        if (!HandleFallMovement())
        {
            return;
        }

        // 공격 처리
        HandleFallAttack();
    }

    /// <summary>
    /// 착지 감지 로직
    /// </summary>
    private bool DetectLanding()
    {
        _groundRay2D.Cast();
        bool isGroundedNow = _groundRay2D.Performed;

        // 원웨이 플랫폼에서 아래 점프 중인 경우 착지 무시
        if (IsIgnoringOneWayPlatform(isGroundedNow))
        {
            return false;
        }
        
        // 착지 순간 감지 및 애니메이션 트리거
        if (IsLandingMoment(isGroundedNow))
        {
            TriggerLandingAnimation();
        }
        
        // 착지 확인 (일정 시간 동안 지속적으로 땅에 닿아있어야 함)
        bool isLandingConfirmed = ConfirmLanding(isGroundedNow);

        if (isLandingConfirmed && _owner.PhotonView.IsMine)
        {
            PlayerEventManager.Instance.GetEvents(_owner.ActorNumber).InvokeOnLanded();
        }

        _wasGroundedLastFrame = isGroundedNow;
        return isLandingConfirmed;
    }

    /// <summary>
    /// 낙하 중 이동 처리 (러닝 상태에 따른 분기)
    /// </summary>
    private bool ProcessFallMovement()
    {
        // 러닝 상태에 따른 이동 처리
        if (_owner.PlayerStat.IsRunning)
        {
            HandleRunningMovement();
        }
        else
        {
            HandleNormalMovement();
        }

        // 물리 기반 이동 적용
        ApplyHorizontalMovement();

        // 폭탄 대시 처리
        if (TryBombDash())
        {
            return true;
        }

        // 점프 대시 처리
        return !TryJumpDash();
    }
    
    /// <summary>
    /// Y축 속도를 제한하여 너무 빠르게 떨어지는 것을 방지
    /// </summary>
    private void LimitYVelocity()
    {
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        
        // 낙하 속도 제한 (음수)
        if (velocity.y < MAX_FALL_SPEED)
        {
            velocity.y = MAX_FALL_SPEED;
        }
        
        _owner.Rigidbody2D.linearVelocity = velocity;
    }

    /// <summary>
    /// 낙하 중 공격 처리
    /// </summary>
    private void ProcessFallAttack()
    {
        // 일반 폭탄 공격
        if (InputHandler.GetKeyDown(KeyCode.Z) && CanNormalBomb())
        {
            HandleNormalBombAttack();
        }
        
        // 특수 폭탄 공격
        if (InputHandler.GetKeyDown(KeyCode.X) && CanSpecialBomb())
        {
            HandleSpecialBombAttack();
        }

        if(InputHandler.GetKeyDown(KeyCode.C))
        {
            _owner.ExecuteUltimate();
        }
    }

    // ====== 새로 추가된 헬퍼 메서드들 ======

    /// <summary>
    /// 낙하 애니메이션 설정
    /// </summary>
    private void SetupFallAnimation()
    {
        _owner.RPC_SetAnimatorBool("LandBool", false);
        _owner.RPC_SetAnimatorTrigger("Fall");
    }

    /// <summary>
    /// 점프 카운트 증가 처리 (특정 상태에서만)
    /// </summary>
    private void HandleJumpCountIncrement()
    {
        // 점프 관련 상태에서 떨어지면 점프 횟수 증가하지 않음
        if (!IsFromJumpRelatedState())
        {
            _owner.PlayerStat.IncrementJumpCount();
        }
    }

    /// <summary>
    /// 착지 감지 초기화
    /// </summary>
    private void InitializeLandingDetection()
    {
        _groundRay2D.Cast();
        bool isCurrentlyGrounded = _groundRay2D.Performed;
        _wasGroundedLastFrame = isCurrentlyGrounded;
        _landingTimer = 0f;
    }

    /// <summary>
    /// 낙하 상태 초기화
    /// </summary>
    private void InitializeFallState()
    {
        _fallTimer = 0f;
        _keyReleaseTimer = 0f;
        _originalGravityScale = _owner.Rigidbody2D.gravityScale;
        _lastFacingDirection = DIRECTION_NONE;
    }

    /// <summary>
    /// 타이머 업데이트
    /// </summary>
    private void UpdateTimers()
    {
        _fallTimer += Time.deltaTime;
        
        if (_explosionOverrideTimer > 0f)
        {
            _explosionOverrideTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 점프 상태로 전환 확인
    /// </summary>
    private bool CheckForJumpTransition()
    {
        if (_owner.Rigidbody2D.linearVelocity.y > JUMP_DETECTION_THRESHOLD)
        {
            _playerFSM.ChangeState<PlayerJumpState>();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 착지 상태로 전환 확인
    /// </summary>
    private bool CheckForLandingTransition()
    {
        if (DetectLanding())
        {
            PlayerIdleState.SetLandingFromJump();
            _playerFSM.ChangeState<PlayerIdleState>();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 낙하 중 이동 처리
    /// </summary>
    private bool HandleFallMovement()
    {
        return ProcessFallMovement();
    }

    /// <summary>
    /// 낙하 중 공격 처리
    /// </summary>
    private void HandleFallAttack()
    {
        ProcessFallAttack();
    }

    /// <summary>
    /// 점프 관련 상태에서 온 것인지 확인
    /// </summary>
    private bool IsFromJumpRelatedState()
    {
        return _playerFSM.IsPreviousState<PlayerJumpState>() ||
               _playerFSM.IsPreviousState<PlayerJumpDashState>() ||
               _playerFSM.IsPreviousState<PlayerRecoilState>() ||
               _playerFSM.IsPreviousState<PlayerNormalRecoilState>();
    }

    /// <summary>
    /// 원웨이 플랫폼 무시 확인
    /// </summary>
    private bool IsIgnoringOneWayPlatform(bool isGroundedNow)
    {
        if (!_owner.PlayerStat.IsDownJump || !isGroundedNow)
        {
            return false;
        }

        RaycastHit2D hit = _groundRay2D.Hit;
        return hit.collider != null && hit.collider.CompareTag("OneWayPlatform");
    }

    /// <summary>
    /// 착지 순간인지 확인
    /// </summary>
    private bool IsLandingMoment(bool isGroundedNow)
    {
        return !_wasGroundedLastFrame && isGroundedNow;
    }

    /// <summary>
    /// 착지 애니메이션 트리거
    /// </summary>
    private void TriggerLandingAnimation()
    {
        _owner.RPC_SetAnimatorBool("LandBool", true);
        _landingTimer = 0f;
    }

    /// <summary>
    /// 착지 확인 (일정 시간 동안 지속적으로 땅에 닿아있어야 함)
    /// </summary>
    private bool ConfirmLanding(bool isGroundedNow)
    {
        if (isGroundedNow)
        {
            _landingTimer += Time.deltaTime;
            return _landingTimer >= LANDING_CONFIRM_TIME;
        }
        else
        {
            _landingTimer = 0f;
            return false;
        }
    }

    /// <summary>
    /// 러닝 상태에서의 이동 처리
    /// </summary>
    private void HandleRunningMovement()
    {
        bool isMovingInSameDirection = IsMovingInSameDirection();
        bool isMovingInOppositeDirection = IsMovingInOppositeDirection();

        if (isMovingInSameDirection)
        {
            // 같은 방향으로 이동 중 (공중에서는 러닝 속도에 배율 적용)
            _keyReleaseTimer = 0;
            _horizontalVelocity = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.RunSpeed * AIR_RUNNING_MULTIPLIER;
        }
        else if (isMovingInOppositeDirection)
        {
            // 반대 방향 키 입력 시 러닝 해제 및 방향 전환
            StopRunningAndChangeDirection();
        }
        else
        {
            // 키 입력 없음
            HandleRunningKeyRelease();
        }
    }

    /// <summary>
    /// 일반 이동 상태에서의 이동 처리
    /// </summary>
    private void HandleNormalMovement()
    {
        if (InputHandler.GetKey(KeyCode.RightArrow))
        {
            HandleRightMovement();
        }
        else if (InputHandler.GetKey(KeyCode.LeftArrow))
        {
            HandleLeftMovement();
        }
        else
        {
            _horizontalVelocity = 0;
        }
        
        // 공중에서는 이동 속도에 배율 적용
        _horizontalVelocity *= _owner.PlayerStat.MyMoveSpeed * AIR_CONTROL_MULTIPLIER;
    }

    /// <summary>
    /// 수평 이동 적용
    /// </summary>
    private void ApplyHorizontalMovement()
    {
        if (_explosionOverrideTimer <= 0f)
        {
            Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
            velocity.x = Mathf.Lerp(velocity.x, _horizontalVelocity, VELOCITY_LERP_SPEED * Time.deltaTime);
            _owner.Rigidbody2D.linearVelocity = velocity;
        }
    }

    /// <summary>
    /// 폭탄 대시 시도
    /// </summary>
    private bool TryBombDash()
    {
        if (InputHandler.GetKeyDown(KeyCode.Space) && _owner.PlayerStat.CanJump())
        {
            ExecuteBombDash();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 점프 대시 시도
    /// </summary>
    private bool TryJumpDash()
    {
        return TryLeftJumpDash() || TryRightJumpDash();
    }

    /// <summary>
    /// 일반 폭탄 공격 처리
    /// </summary>
    private void HandleNormalBombAttack()
    {
        if (_owner.PlayerStat.IsRunning)
        {
            ThrowStraightNormalBomb();
            _playerFSM.ChangeState<PlayerRecoilState>();
        }
        else
        {
            if (HasDirectionalInput())
            {
                ThrowNormalBomb();
            }
            else
            {
                PlaceNormalBomb();
            }
            _playerFSM.ChangeState<PlayerNormalRecoilState>();
        }
    }

    /// <summary>
    /// 특수 폭탄 공격 처리
    /// </summary>
    private void HandleSpecialBombAttack()
    {
        if (_owner.PlayerStat.IsRunning)
        {
            ThrowStraightSpecialBomb();
            _playerFSM.ChangeState<PlayerRecoilState>();
        }
        else
        {
            if (HasDirectionalInput())
            {
                ThrowSpecialBomb();
            }
            else
            {
                PlaceSpecialBomb();
            }
            _playerFSM.ChangeState<PlayerNormalRecoilState>();
        }
    }

    /// <summary>
    /// 같은 방향으로 이동 중인지 확인
    /// </summary>
    private bool IsMovingInSameDirection()
    {
        return (InputHandler.GetKey(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == DIRECTION_RIGHT) ||
               (InputHandler.GetKey(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == DIRECTION_LEFT);
    }

    /// <summary>
    /// 반대 방향으로 이동 중인지 확인
    /// </summary>
    private bool IsMovingInOppositeDirection()
    {
        return (InputHandler.GetKey(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == DIRECTION_LEFT) ||
               (InputHandler.GetKey(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == DIRECTION_RIGHT);
    }

    /// <summary>
    /// 러닝 중단 및 방향 전환
    /// </summary>
    private void StopRunningAndChangeDirection()
    {
        _owner.PlayerStat.IsRunning = false;
        _owner.RPC_SetFacingDirection(-_owner.PlayerStat.FacingDirection);
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
        _keyReleaseTimer = 0;
    }

    /// <summary>
    /// 러닝 중 키 릴리즈 처리
    /// </summary>
    private void HandleRunningKeyRelease()
    {
        _keyReleaseTimer += Time.deltaTime;
        
        if (_keyReleaseTimer >= KEY_RELEASE_THRESHOLD)
        {
            _owner.PlayerStat.IsRunning = false;
            _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
        }
        else
        {
            _horizontalVelocity = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.RunSpeed * AIR_RUNNING_MULTIPLIER;
        }
    }

    /// <summary>
    /// 오른쪽 이동 처리
    /// </summary>
    private void HandleRightMovement()
    {
        if (_owner.PlayerStat.FacingDirection == DIRECTION_LEFT)
        {
            _owner.PlayerStat.IsRunning = false;
            _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
        }
        
        _horizontalVelocity = 1;
        UpdateFacingDirection(DIRECTION_RIGHT);
    }

    /// <summary>
    /// 왼쪽 이동 처리
    /// </summary>
    private void HandleLeftMovement()
    {
        if (_owner.PlayerStat.FacingDirection == DIRECTION_RIGHT)
        {
            _owner.PlayerStat.IsRunning = false;
            _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
        }
        
        _horizontalVelocity = -1;
        UpdateFacingDirection(DIRECTION_LEFT);
    }

    /// <summary>
    /// 바라보는 방향 업데이트 (방향이 바뀔 때만 RPC 호출)
    /// </summary>
    private void UpdateFacingDirection(int newDirection)
    {
        if (_lastFacingDirection != newDirection)
        {
            _owner.RPC_SetFacingDirection(newDirection);
            _lastFacingDirection = newDirection;
        }
    }

    /// <summary>
    /// 폭탄 대시 실행
    /// </summary>
    private void ExecuteBombDash()
    {
        _owner.PlayerStat.IncrementJumpCount();

        Vector3 position = _owner.GetExplosionSpawnPoint().position;
        GameObject prefab = PhotonNetwork.Instantiate("BasicBomb", position, Quaternion.identity);
        
        if (prefab.TryGetComponent(out Bomb bomb) && _owner.PhotonView.IsMine)
        {
            bomb.PhotonView.RPC(nameof(bomb.SetOwner), RpcTarget.All, _owner.PhotonView.ViewID);
            bomb.PhotonView.RPC(nameof(bomb.Explode), RpcTarget.All);
            _explosionOverrideTimer = EXPLOSION_OVERRIDE_DURATION;
        }
    }

    /// <summary>
    /// 왼쪽 점프 대시 시도
    /// </summary>
    private bool TryLeftJumpDash()
    {
        if (InputHandler.GetKeyDown(KeyCode.LeftArrow))
        {
            _owner.LastDashTapTimeLeft = Time.time;
            
            if (IsDoubleTap(_lastLeftTapTime) && _owner.PlayerStat.CanJumpDash())
            {
                _owner.RPC_SetFacingDirection(DIRECTION_LEFT);
                _playerFSM.ChangeState<PlayerJumpDashState>();
                return true;
            }
            
            _lastLeftTapTime = Time.time;
        }
        return false;
    }

    /// <summary>
    /// 오른쪽 점프 대시 시도
    /// </summary>
    private bool TryRightJumpDash()
    {
        if (InputHandler.GetKeyDown(KeyCode.RightArrow))
        {
            _owner.LastDashTapTimeRight = Time.time;
            
            if (IsDoubleTap(_lastRightTapTime) && _owner.PlayerStat.CanJumpDash())
            {
                _owner.RPC_SetFacingDirection(DIRECTION_RIGHT);
                _playerFSM.ChangeState<PlayerJumpDashState>();
                return true;
            }
            
            _lastRightTapTime = Time.time;
        }
        return false;
    }

    /// <summary>
    /// 더블 탭 확인
    /// </summary>
    private bool IsDoubleTap(float lastTapTime)
    {
        return Time.time - lastTapTime <= _owner.PlayerStat.DoubleTapTime;
    }
}
