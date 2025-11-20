using UnityEngine;

/// <summary>
/// 플레이어 혼란(Confuse) 상태 클래스
/// 
/// 역할:
/// - 플레이어가 혼란 상태일 때 무작위로 좌우 이동하는 상태 처리
/// - 일정 시간 동안 자동으로 방향을 변경하며 이동
/// - 벽에 막혔을 때 자동으로 방향 전환
/// 
/// 동작 방식:
/// 1. 혼란 지속 시간 동안 런 속도로 자동 이동
/// 2. 무작위 간격으로 방향 변경 (1~4초)
/// 3. 벽에 막혔을 때 즉시 방향 전환
/// 4. 혼란 시간 완료 시 Idle 상태로 전환
/// </summary>
public class PlayerConfuseState : PlayerBaseState
{
    // 방향 변경 관련 상수
    private const float DIRECTION_CHANGE_MIN_INTERVAL = 1f;    // 방향 변경 최소 간격 (초)
    private const float DIRECTION_CHANGE_MAX_INTERVAL = 4f;    // 방향 변경 최대 간격 (초)
    private const float MIN_CHANGE_COOLDOWN = 0.25f;           // 방향 전환 최소 쿨다운 (초)
    private const float SPEED_EPSILON = 0.05f;                // 벽 충돌 판정 속도 임계값
    private const float RANDOM_DIRECTION_THRESHOLD = 0.5f;    // 무작위 방향 결정 임계값
    
    // 방향 상수
    private const float DIRECTION_LEFT = -1f;
    private const float DIRECTION_RIGHT = 1f;
    
    // 상태 변수들
    private float _confuseTimer = 0f;                // 혼란 지속 시간 타이머
    private float _directionChangeTimer = 0f;        // 방향 변경 타이머
    private float _directionChangeInterval = 0f;     // 현재 방향 변경 간격
    private float _currentDirection = DIRECTION_RIGHT; // 현재 이동 방향
    private float _timeSinceLastDirectionChange = 0f; // 마지막 방향 변경 이후 경과 시간
    /// <summary>
    /// 혼란 상태 진입 시 초기화
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();

        if (_owner.PhotonView.IsMine)
        {
            InputHandler.BlockInput = true;
        }
        
        // 타이머 초기화
        InitializeTimers();
        
        // 플레이어 상태 설정 (런 속도로 이동)
        SetupPlayerMovementState();
        
        // 현재 바라보는 방향을 기준으로 초기 방향 설정
        _currentDirection = _owner.PlayerStat.FacingDirection >= 0 ? DIRECTION_RIGHT : DIRECTION_LEFT;

        // 애니메이션 설정
        _owner.ResetAnimatorTrigger("Idle");
        _owner.RPC_SetAnimatorTrigger("Confuse");
    }

    /// <summary>
    /// 혼란 상태 종료 시 정리 작업
    /// </summary>
    public override void OnExit()
    {
        base.OnExit();
        _owner.RPC_ResetAnimatorTrigger("Confuse");
        if (_owner.PhotonView.IsMine)
        {
            InputHandler.BlockInput = false;
        }
    }

    /// <summary>
    /// 혼란 상태의 메인 업데이트 로직
    /// </summary>
    public override void MineUpdate()
    {
        UpdateTimers();

        // 혼란 시간 완료 체크
        if (IsConfuseTimeExpired())
        {
            SyncStateChange<PlayerIdleState>();
            return;
        }

        // 벽 충돌 체크 및 방향 전환
        CheckForWallCollisionAndChangeDirection();

        // 무작위 방향 변경 체크
        CheckForRandomDirectionChange();

        // 이동 적용
        ApplyConfuseMovement();
    }

    /// <summary>
    /// 타이머들을 초기화
    /// </summary>
    private void InitializeTimers()
    {
        _confuseTimer = 0f;
        _directionChangeTimer = 0f;
        _timeSinceLastDirectionChange = 0f;
        _directionChangeInterval = Random.Range(DIRECTION_CHANGE_MIN_INTERVAL, DIRECTION_CHANGE_MAX_INTERVAL);
    }

    /// <summary>
    /// 플레이어 이동 상태를 설정 (런 속도로 설정)
    /// </summary>
    private void SetupPlayerMovementState()
    {
        _owner.PlayerStat.IsRunning = true;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.RunSpeed;
    }

    /// <summary>
    /// 모든 타이머 업데이트
    /// </summary>
    private void UpdateTimers()
    {
        _confuseTimer += Time.deltaTime;
        _directionChangeTimer += Time.deltaTime;
        _timeSinceLastDirectionChange += Time.deltaTime;
    }

    /// <summary>
    /// 혼란 시간이 만료되었는지 확인
    /// </summary>
    private bool IsConfuseTimeExpired()
    {
        return _confuseTimer >= _owner.PlayerStat.ConfuseTime;
    }

    /// <summary>
    /// 벽 충돌 감지 및 방향 전환
    /// </summary>
    private void CheckForWallCollisionAndChangeDirection()
    {
        if (!CanChangeDirection())
        {
            return;
        }

        float currentSpeed = Mathf.Abs(_owner.Rigidbody2D.linearVelocity.x);
        
        // 속도가 임계값 이하라면 벽에 막힌 것으로 판단
        if (currentSpeed <= SPEED_EPSILON)
        {
            ChangeDirection();
        }
    }

    /// <summary>
    /// 무작위 방향 변경 체크
    /// </summary>
    private void CheckForRandomDirectionChange()
    {
        if (_directionChangeTimer >= _directionChangeInterval)
        {
            ChangeDirectionRandomly();
        }
    }

    /// <summary>
    /// 방향 변경이 가능한지 확인 (쿨다운 체크)
    /// </summary>
    private bool CanChangeDirection()
    {
        return _timeSinceLastDirectionChange >= MIN_CHANGE_COOLDOWN;
    }

    /// <summary>
    /// 현재 방향을 반대로 변경
    /// </summary>
    private void ChangeDirection()
    {
        _currentDirection = _currentDirection == DIRECTION_RIGHT ? DIRECTION_LEFT : DIRECTION_RIGHT;
        ResetDirectionChangeTimers();
        UpdatePlayerFacingDirection();
    }

    /// <summary>
    /// 무작위로 방향 변경
    /// </summary>
    private void ChangeDirectionRandomly()
    {
        _currentDirection = Random.value < RANDOM_DIRECTION_THRESHOLD ? DIRECTION_LEFT : DIRECTION_RIGHT;
        ResetDirectionChangeTimers();
        SetNewRandomInterval();
        UpdatePlayerFacingDirection();
    }

    /// <summary>
    /// 방향 변경 관련 타이머들을 리셋
    /// </summary>
    private void ResetDirectionChangeTimers()
    {
        _directionChangeTimer = 0f;
        _timeSinceLastDirectionChange = 0f;
    }

    /// <summary>
    /// 새로운 무작위 방향 변경 간격 설정
    /// </summary>
    private void SetNewRandomInterval()
    {
        _directionChangeInterval = Random.Range(DIRECTION_CHANGE_MIN_INTERVAL, DIRECTION_CHANGE_MAX_INTERVAL);
    }

    /// <summary>
    /// 플레이어의 바라보는 방향 업데이트
    /// </summary>
    private void UpdatePlayerFacingDirection()
    {
        int facingDirection = _currentDirection > 0 ? 1 : -1;
        _owner.RPC_SetFacingDirection(facingDirection);
    }

    /// <summary>
    /// 혼란 상태 이동 적용 (런 속도로 좌우 이동, 중력은 유지)
    /// </summary>
    private void ApplyConfuseMovement()
    {
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        velocity.x = _owner.PlayerStat.RunSpeed * _currentDirection;
        _owner.Rigidbody2D.linearVelocity = velocity;
    }
}
