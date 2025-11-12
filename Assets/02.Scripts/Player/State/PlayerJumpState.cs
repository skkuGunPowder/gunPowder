using System;
using Photon.Pun;
using RobustFSM.Base;
using UnityEngine;

/// <summary>
/// 플레이어 점프 상태 클래스
/// 
/// 역할:
/// - 플레이어가 점프하여 상승하는 동안의 처리
/// - 공중에서의 좌우 이동 및 러닝 상태 관리
/// - 점프 최고점 감지 및 낙하 상태로 전환
/// - 공중 공격 (폭탄 설치/투척) 처리
/// - 공중 대시 (폭탄 대시, 점프 대시) 처리
/// 
/// 동작 방식:
/// 1. 점프 시작 시 Y축 속도 설정 및 점프 카운트 증가
/// 2. 상승 중 좌우 이동 처리 (러닝/일반 이동)
/// 3. Y축 속도 제한으로 최대 점프 속도 제어
/// 4. 점프 최고점 감지 후 낙하 상태로 전환
/// 5. 공중 공격 및 대시 입력 처리
/// </summary>
public class PlayerJumpState : PlayerBaseState
{
    // 물리 관련 상수
    private const float MAX_JUMP_SPEED = 40f;                   // 최대 점프 속도 (양수)
    private const float VELOCITY_LERP_SPEED = 3f;               // 속도 보간 속도 (높을수록 반응 빠름)
    
    // 공중 제어 관련 상수 (FallState와 동일한 느낌을 위해)
    private const float AIR_CONTROL_MULTIPLIER = 1.5f;            // 공중 이동 속도 배율 (지상 대비)
    private const float AIR_RUNNING_MULTIPLIER = 1.5f;            // 공중 러닝 속도 배율 (지상 러닝 대비)
    
    // 타이머 관련 상수
    private const float KEY_RELEASE_THRESHOLD = 0.3f;           // 키 릴리즈 임계값 (초)
    private const float EXPLOSION_OVERRIDE_DURATION = 0.3f;     // 폭발 후 X축 속도 덮어쓰기 차단 시간 (초)
    
    // 방향 상수
    private const int DIRECTION_LEFT = -1;                      // 왼쪽 방향
    private const int DIRECTION_RIGHT = 1;                      // 오른쪽 방향
    private const int DIRECTION_NONE = 0;                       // 방향 없음
    
    // 이동 관련 변수들
    private float _verticalVelocity = 0f;                       // 수직 이동 속도
    private float _horizontalVelocity = 0f;                     // 수평 이동 속도
    private float _jumpTimer = 0f;                              // 점프 시간 타이머
    private float _originalGravityScale;                        // 원본 중력 스케일
    
    // 입력 관련 변수들
    private float _keyReleaseTimer = 0f;                        // 키 릴리즈 타이머
    private float _lastLeftTapTime = 0f;                        // 마지막 왼쪽 키 탭 시간
    private float _lastRightTapTime = 0f;                       // 마지막 오른쪽 키 탭 시간
    private int _lastFacingDirection = DIRECTION_NONE;          // 마지막 바라본 방향
    
    // 폭발 효과 관련 변수들
    private float _explosionOverrideTimer = 0f;                 // 폭발 후 X축 속도 덮어쓰기 차단 타이머
    
    // 점프 최고점 감지 관련 변수들
    private float _lastYVelocity = 0f;                          // 이전 프레임의 Y축 속도
    private bool _hasReachedPeak = false;                       // 점프 최고점 도달 여부

    /// <summary>
    /// 점프 상태 진입 시 초기화
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();

        // 애니메이션 설정
        SetupJumpAnimation();

        // 플레이어 상태 설정
        _owner.PlayerStat.IsJumping = true;
        
        // 점프 실행 (낙하 상태에서 온 경우 제외)
        HandleJumpExecution();

        // 타이머 및 설정 초기화
        InitializeJumpState();

        // 점프 사운드 재생
        PlayJumpSound();
    }

    /// <summary>
    /// 점프 상태 종료 시 정리 작업
    /// </summary>
    public override void OnExit()
    {
        // 애니메이션 정리
        _owner.RPC_ResetAnimatorTrigger("Jump");
        
        // 물리 설정 복원
        _owner.Rigidbody2D.gravityScale = _originalGravityScale;
        
        // 점프 상태는 낙하 상태에서도 유지 (주석 처리된 이유)
        // _owner.PlayerStat.IsJumping = false;

        base.OnExit();
    }

    /// <summary>
    /// 점프 상태의 메인 업데이트 로직
    /// </summary>
    public override void MineUpdate()
    {
        // 타이머 업데이트
        UpdateTimers();

        // Y축 속도 제한 적용
        LimitYVelocity();

        // 점프 최고점 감지 및 낙하 상태로 전환
        if (CheckForPeakTransition())
        {
            return;
        }

        // 이동 처리
        if (!HandleJumpMovement())
        {
            return;
        }

        // 공격 처리
        HandleJumpAttack();
    }

    /// <summary>
    /// 점프 최고점 감지 로직
    /// </summary>
    private bool DetectJumpPeak()
    {
        float currentYVelocity = _owner.Rigidbody2D.linearVelocity.y;
        
        // 이전 프레임에서 양수였는데 지금 음수가 되면 최고점 도달
        if (_lastYVelocity > 0f && currentYVelocity <= 0f && !_hasReachedPeak)
        {
            _hasReachedPeak = true;
            return true; // FallState로 전환
        }
        
        _lastYVelocity = currentYVelocity;
        return false;
    }

    /// <summary>
    /// 점프 중 이동 처리 (러닝 상태에 따른 분기)
    /// </summary>
    private bool ProcessJumpMovement()
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
    /// Y축 속도를 제한하여 너무 빠르게 올라가는 것을 방지
    /// </summary>
    private void LimitYVelocity()
    {
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        
        // 점프 속도 제한 (양수)
        if (velocity.y > MAX_JUMP_SPEED)
        {
            velocity.y = MAX_JUMP_SPEED;
        }
        
        _owner.Rigidbody2D.linearVelocity = velocity;
    }

    /// <summary>
    /// 점프 중 공격 처리
    /// </summary>
    private void ProcessJumpAttack()
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
    }

    // ====== 새로 추가된 헬퍼 메서드들 ======

    /// <summary>
    /// 점프 애니메이션 설정
    /// </summary>
    private void SetupJumpAnimation()
    {
        _owner.RPC_SetAnimatorBool("LandBool", false);
        _owner.RPC_SetAnimatorTrigger("Jump");
    }

    /// <summary>
    /// 점프 실행 처리 (낙하 상태에서 온 경우 제외)
    /// </summary>
    private void HandleJumpExecution()
    {
        _verticalVelocity = 0;
        
        if (!_playerFSM.IsPreviousState<PlayerFallState>())
        {
            _owner.PlayerStat.IncrementJumpCount();
            _verticalVelocity = _owner.PlayerStat.JumpForce;
            
            // 점프 시에만 Y축 속도 설정
            Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
            velocity.y = _verticalVelocity;
            _owner.Rigidbody2D.linearVelocity = velocity;
        }
    }

    /// <summary>
    /// 점프 상태 초기화
    /// </summary>
    private void InitializeJumpState()
    {
        _jumpTimer = 0f;
        _keyReleaseTimer = 0f;
        _originalGravityScale = _owner.Rigidbody2D.gravityScale;
        _hasReachedPeak = false;
        _lastYVelocity = _owner.Rigidbody2D.linearVelocity.y;
        _lastFacingDirection = DIRECTION_NONE;
    }

    /// <summary>
    /// 점프 사운드 재생
    /// </summary>
    private void PlayJumpSound()
    {
        SoundManager.Instance.PlayLocalSound("PlayerJump_1", transform);
    }

    /// <summary>
    /// 타이머 업데이트
    /// </summary>
    private void UpdateTimers()
    {
        _jumpTimer += Time.deltaTime;
        
        if (_explosionOverrideTimer > 0f)
        {
            _explosionOverrideTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 점프 최고점 전환 확인
    /// </summary>
    private bool CheckForPeakTransition()
    {
        if (DetectJumpPeak())
        {
            _playerFSM.ChangeState<PlayerFallState>();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 점프 중 이동 처리
    /// </summary>
    private bool HandleJumpMovement()
    {
        return ProcessJumpMovement();
    }

    /// <summary>
    /// 점프 중 공격 처리
    /// </summary>
    private void HandleJumpAttack()
    {
        ProcessJumpAttack();
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