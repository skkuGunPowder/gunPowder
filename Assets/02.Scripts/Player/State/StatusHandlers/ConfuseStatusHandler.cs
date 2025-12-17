using UnityEngine;
using System;

/// <summary>
/// 혼란 상태이상 핸들러
/// PlayerConfuseState의 로직을 이곳으로 이동
/// QTE 시스템을 통해 혼란 상태를 해제할 수 있음
/// </summary>
public class ConfuseStatusHandler : IStatusEffectHandler
{
    public StatusEffectType StatusType => StatusEffectType.Confuse;
    
    // 방향 변경 관련 상수
    private const float DIRECTION_CHANGE_MIN_INTERVAL = 0.3f;    // 방향 변경 최소 간격 (초)
    private const float DIRECTION_CHANGE_MAX_INTERVAL = 1f;    // 방향 변경 최대 간격 (초)
    private const float MIN_CHANGE_COOLDOWN = 0.1f;           // 방향 전환 최소 쿨다운 (초)
    private const float SPEED_EPSILON = 0.05f;                // 벽 충돌 판정 속도 임계값
    private const float RANDOM_DIRECTION_THRESHOLD = 0.5f;    // 무작위 방향 결정 임계값
    
    // QTE 관련 상수
    private const float MAX_INTENSITY = 10f;  // 최대 강도 (점수 목표)
    private const float SCORE_FIRST_INPUT = 1f;      // 첫 입력 점수
    private const float SCORE_OPPOSITE_INPUT = 1f;   // 반대 방향 입력 점수
    private const float SCORE_SAME_INPUT = 0.5f;    // 같은 방향 입력 점수
    
    // 방향 상수
    private const float DIRECTION_LEFT = -1f;
    private const float DIRECTION_RIGHT = 1f;
    
    // 방향키 타입 enum
    public enum DirectionKeyType
    {
        None = 0,
        Left = -1,
        Right = 1
    }
    
    // 상태 변수들
    private float _confuseTimer = 0f;                // 혼란 지속 시간 타이머
    private float _directionChangeTimer = 0f;        // 방향 변경 타이머
    private float _directionChangeInterval = 0f;     // 현재 방향 변경 간격
    private float _currentDirection = DIRECTION_RIGHT; // 현재 이동 방향
    private float _timeSinceLastDirectionChange = 0f; // 마지막 방향 변경 이후 경과 시간
    
    // QTE 관련 변수들
    private bool _isQTEActive = false;                // QTE 활성화 여부
    private bool _isQTESuccess = false;                // QTE 성공 여부 (혼란 상태 해제)
    private float _currentScore = 0f;                 // 현재 점수
    private DirectionKeyType _currentBaseDirection = DirectionKeyType.None;  // 현재 기준 방향
    private bool _hasReceivedFirstInput = false;      // 첫 입력을 받았는지 여부
    
    // QTE UI 이벤트 (나중에 UI 시스템에서 구독)
    public event Action<float, float> OnQTEProgressChanged;  // (현재 점수, 최대 강도)
    public event Action<DirectionKeyType> OnQTEBaseDirectionChanged;  // 기준 방향 변경
    public event Action OnQTEStarted;  // QTE 시작
    public event Action OnQTEEnded;   // QTE 종료 (성공)
    
    public void OnEnter(Player owner)
    {
        if (owner.PhotonView.IsMine)
        {
            InputHandler.BlockInput = true;
        }
        
        // 타이머 초기화
        InitializeTimers();
        
        // 플레이어 상태 설정 (런 속도로 이동)
        SetupPlayerMovementState(owner);
        
        // 현재 바라보는 방향을 기준으로 초기 방향 설정
        _currentDirection = owner.PlayerStat.FacingDirection >= 0 ? DIRECTION_RIGHT : DIRECTION_LEFT;

        // 애니메이션 설정
        owner.ResetAnimatorTrigger("Idle");
        owner.RPC_SetAnimatorTrigger("Confuse");
        
        // QTE 시스템 초기화 및 시작
        InitializeQTE();
    }
    
    public void OnExit(Player owner)
    {
        owner.RPC_ResetAnimatorTrigger("Confuse");
        if (owner.PhotonView.IsMine)
        {
            InputHandler.BlockInput = false;
        }
        
        // QTE 시스템 정리
        CleanupQTE();
    }
    
    public void Update(Player owner)
    {
        if (!owner.PhotonView.IsMine)
        {
            return;
        }
        
        UpdateTimers();

        // QTE 입력 처리 (QTE가 활성화되어 있을 때)
        if (_isQTEActive)
        {
            ProcessQTEInput(owner);
        }

        // 벽 충돌 체크 및 방향 전환
        CheckForWallCollisionAndChangeDirection(owner);

        // 무작위 방향 변경 체크
        CheckForRandomDirectionChange();

        // 이동 적용
        ApplyConfuseMovement(owner);
    }
    
    public bool IsFinished(Player owner)
    {
        // QTE 성공으로 해제되었거나 시간이 지나면 종료
        return _isQTESuccess || _confuseTimer >= owner.PlayerStat.ConfuseTime;
    }
    
    /// <summary>
    /// 타이머들을 초기화
    /// </summary>
    private void InitializeTimers()
    {
        _confuseTimer = 0f;
        _directionChangeTimer = 0f;
        _timeSinceLastDirectionChange = 0f;
        _directionChangeInterval = UnityEngine.Random.Range(DIRECTION_CHANGE_MIN_INTERVAL, DIRECTION_CHANGE_MAX_INTERVAL);
    }

    /// <summary>
    /// 플레이어 이동 상태를 설정 (런 속도로 설정)
    /// </summary>
    private void SetupPlayerMovementState(Player owner)
    {
        owner.PlayerStat.IsRunning = true;
        owner.PlayerStat.MyMoveSpeed = owner.PlayerStat.RunSpeed;
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
    /// 벽 충돌 감지 및 방향 전환
    /// </summary>
    private void CheckForWallCollisionAndChangeDirection(Player owner)
    {
        if (!CanChangeDirection())
        {
            return;
        }

        float currentSpeed = Mathf.Abs(owner.Rigidbody2D.linearVelocity.x);
        
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
    }

    /// <summary>
    /// 무작위로 방향 변경
    /// </summary>
    private void ChangeDirectionRandomly()
    {
        _currentDirection = UnityEngine.Random.value < RANDOM_DIRECTION_THRESHOLD ? DIRECTION_LEFT : DIRECTION_RIGHT;
        ResetDirectionChangeTimers();
        SetNewRandomInterval();
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
        _directionChangeInterval = UnityEngine.Random.Range(DIRECTION_CHANGE_MIN_INTERVAL, DIRECTION_CHANGE_MAX_INTERVAL);
    }

    /// <summary>
    /// 혼란 상태 이동 적용 (런 속도로 좌우 이동, 중력은 유지)
    /// </summary>
    private void ApplyConfuseMovement(Player owner)
    {
        Vector2 velocity = owner.Rigidbody2D.linearVelocity;
        velocity.x = owner.PlayerStat.RunSpeed * _currentDirection;
        owner.Rigidbody2D.linearVelocity = velocity;
        
        // 플레이어의 바라보는 방향 업데이트
        int facingDirection = _currentDirection > 0 ? 1 : -1;
        owner.RPC_SetFacingDirection(facingDirection);
    }
    
    #region QTE System
    
    /// <summary>
    /// QTE 시스템 초기화
    /// </summary>
    private void InitializeQTE()
    {
        _isQTEActive = true;
        _currentScore = 0f;
        _currentBaseDirection = DirectionKeyType.None;
        _hasReceivedFirstInput = false;
        
        Debug.Log($"[QTE] 혼란 상태 QTE 시작! 목표 점수: {MAX_INTENSITY}점");
        
        // QTE 시작 이벤트 발생
        OnQTEStarted?.Invoke();
    }
    
    /// <summary>
    /// QTE 시스템 정리
    /// </summary>
    private void CleanupQTE()
    {
        if (_isQTEActive)
        {
            _isQTEActive = false;
            Debug.Log($"[QTE] QTE 종료 (시간 초과) - 최종 점수: {_currentScore:F1}/{MAX_INTENSITY}");
            OnQTEEnded?.Invoke();
        }
    }
    
    /// <summary>
    /// QTE 입력 처리
    /// </summary>
    private void ProcessQTEInput(Player owner)
    {
        // 좌우 방향키 입력 감지
        bool leftKeyPressed = Input.GetKeyDown(KeyCode.LeftArrow);
        bool rightKeyPressed = Input.GetKeyDown(KeyCode.RightArrow);
        
        if (!leftKeyPressed && !rightKeyPressed)
        {
            return; // 입력이 없으면 처리하지 않음
        }
        
        // 입력된 방향 결정
        DirectionKeyType inputDirection = leftKeyPressed ? DirectionKeyType.Left : DirectionKeyType.Right;
        
        // 점수 계산 및 기준 방향 업데이트
        ProcessQTEInputLogic(inputDirection);
        
        // 점수가 최대 강도에 도달했는지 확인
        if (_currentScore >= MAX_INTENSITY)
        {
            // QTE 성공 - 혼란 상태 해제
            CompleteQTE();
        }
    }
    
    /// <summary>
    /// QTE 입력 로직 처리
    /// </summary>
    private void ProcessQTEInputLogic(DirectionKeyType inputDirection)
    {
        float scoreBefore = _currentScore;
        string inputDirectionStr = inputDirection == DirectionKeyType.Left ? "← (왼쪽)" : "→ (오른쪽)";
        
        if (!_hasReceivedFirstInput)
        {
            // 첫 입력: 1점을 주고 기준 방향 설정
            _currentScore += SCORE_FIRST_INPUT;
            _currentBaseDirection = inputDirection;
            _hasReceivedFirstInput = true;
            
            Debug.Log($"[QTE] 첫 입력: {inputDirectionStr} | 점수: {scoreBefore:F1} → {_currentScore:F1} (+{SCORE_FIRST_INPUT}) | 기준 방향: {inputDirectionStr}");
            
            // 기준 방향 변경 이벤트 발생
            OnQTEBaseDirectionChanged?.Invoke(_currentBaseDirection);
        }
        else
        {
            // 두 번째 입력부터
            string baseDirectionStr = _currentBaseDirection == DirectionKeyType.Left ? "← (왼쪽)" : "→ (오른쪽)";
            
            if (inputDirection != _currentBaseDirection)
            {
                // 반대 방향 입력: 1점을 주고 기준을 반대 방향으로 변경
                _currentScore += SCORE_OPPOSITE_INPUT;
                _currentBaseDirection = inputDirection;
                
                Debug.Log($"[QTE] 반대 방향 입력: {inputDirectionStr} (기준: {baseDirectionStr}) | 점수: {scoreBefore:F1} → {_currentScore:F1} (+{SCORE_OPPOSITE_INPUT}) | 기준 변경: {baseDirectionStr} → {inputDirectionStr}");
                
                // 기준 방향 변경 이벤트 발생
                OnQTEBaseDirectionChanged?.Invoke(_currentBaseDirection);
            }
            else
            {
                // 같은 방향 입력: 0.5점을 주고 기준 유지
                _currentScore += SCORE_SAME_INPUT;
                
                Debug.Log($"[QTE] 같은 방향 입력: {inputDirectionStr} (기준: {baseDirectionStr}) | 점수: {scoreBefore:F1} → {_currentScore:F1} (+{SCORE_SAME_INPUT}) | 기준 유지");
            }
        }
        
        // 진행률 로그
        float progress = (_currentScore / MAX_INTENSITY) * 100f;
        Debug.Log($"[QTE] 진행률: {_currentScore:F1}/{MAX_INTENSITY} ({progress:F1}%)");
        
        // 점수 변경 이벤트 발생 (UI 업데이트용)
        OnQTEProgressChanged?.Invoke(_currentScore, MAX_INTENSITY);
    }
    
    /// <summary>
    /// QTE 완료 처리
    /// </summary>
    private void CompleteQTE()
    {
        _isQTEActive = false;
        _isQTESuccess = true;
        
        Debug.Log($"[QTE] ★ QTE 성공! 혼란 상태 해제 ★ | 최종 점수: {_currentScore:F1}/{MAX_INTENSITY}");
        
        OnQTEEnded?.Invoke();
    }
    
    #endregion
}

