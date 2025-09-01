using RobustFSM.Base;
using UnityEngine;

/// <summary>
/// 플레이어 브레이크 상태 클래스
/// 
/// 역할:
/// - 이동 중 급정거 시 발생하는 브레이크 상태 처리
/// - 브레이크 중 반대 방향 키 입력 시 런 상태로 전환 준비
/// - 브레이크 시간 완료 후 적절한 상태로 전환 (Idle/Fall/Run)
/// 
/// 동작 방식:
/// 1. 브레이크 진입 시 현재 방향으로 감속 이동
/// 2. 브레이크 시간 내 반대 방향 키 입력 시 더블탭으로 인식
/// 3. 브레이크 완료 시 더블탭 여부에 따라 Run 또는 Idle/Fall 상태로 전환
/// </summary>
public class PlayerBreakState : PlayerBaseState
{
    // 상수 정의
    private const float BREAK_SPEED_MULTIPLIER = 0.5f; // 브레이크 시 속도 배율
    private const int DEFAULT_FACING_DIRECTION = 1;
    
    // 상태 변수들
    private float _breakTimer = 0f;
    private int _breakMoveDirection = DEFAULT_FACING_DIRECTION; // 브레이크 시 이동 방향
    private bool _canDetectDoubleTap = true; // 더블탭 감지 가능 여부
    private bool _hasDoubleTapped = false; // 더블탭 발생 여부

    /// <summary>
    /// 브레이크 상태 진입 시 초기화
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();

        // 타이머 및 상태 변수 초기화
        _breakTimer = 0f;
        _breakMoveDirection = _owner.PlayerStat.FacingDirection;
        _canDetectDoubleTap = true;
        _hasDoubleTapped = false;

        // 플레이어 상태 설정
        _owner.PlayerStat.IsRunning = false;

        // 브레이크 애니메이션 재생
        _owner.RPC_SetAnimatorTrigger("Break");
    }

    /// <summary>
    /// 브레이크 상태 종료 시 정리 작업
    /// </summary>
    public override void OnExit()
    {
        base.OnExit();
        
        // 브레이크 완료 후 방향 전환 (브레이크는 반대 방향으로 진행되므로)
        _owner.RPC_SetFacingDirection(-_breakMoveDirection);
        
        // 브레이크 애니메이션 리셋
        _owner.RPC_ResetAnimatorTrigger("Break");
    }

    /// <summary>
    /// 브레이크 상태의 메인 업데이트 로직
    /// </summary>
    public override void MineUpdate()
    {
        _breakTimer += Time.deltaTime;

        // 브레이크 이동 처리 (감속된 속도로 이동)
        ApplyBreakMovement();

        // 더블탭 입력 체크 (런 상태 전환을 위한 조건 검사)
        CheckForDoubleTap();

        // 브레이크 시간 완료 시 다음 상태로 전환
        if(_breakTimer >= _owner.PlayerStat.BreakTime)
        {
            TransitionToNextState();
        }
    }

    /// <summary>
    /// 브레이크 시 이동 처리 (감속된 속도로 이동)
    /// </summary>
    private void ApplyBreakMovement()
    {
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        velocity.x = _breakMoveDirection * _owner.PlayerStat.MyMoveSpeed * BREAK_SPEED_MULTIPLIER;
        _owner.Rigidbody2D.linearVelocity = velocity;
    }

    /// <summary>
    /// 더블탭 입력 체크 (브레이크 중 반대 방향 키 입력 감지)
    /// </summary>
    private void CheckForDoubleTap()
    {
        if (!_canDetectDoubleTap || _breakTimer > _owner.PlayerStat.DoubleTapTime)
        {
            return;
        }

        if (IsOppositeDirectionKeyPressed())
        {
            _hasDoubleTapped = true;
            _canDetectDoubleTap = false; // 더 이상 체크하지 않음
        }
    }

    /// <summary>
    /// 현재 진행 방향과 반대 방향 키가 눌렸는지 확인
    /// </summary>
    private bool IsOppositeDirectionKeyPressed()
    {
        bool rightKeyPressed = InputHandler.GetKeyDown(KeyCode.RightArrow);
        bool leftKeyPressed = InputHandler.GetKeyDown(KeyCode.LeftArrow);
        int currentFacingDirection = _owner.PlayerStat.FacingDirection;

        // 현재 오른쪽을 보고 있는데 왼쪽 키를 눌렀거나, 왼쪽을 보고 있는데 오른쪽 키를 눌렀을 때
        return (rightKeyPressed && currentFacingDirection == -1) || 
               (leftKeyPressed && currentFacingDirection == 1);
    }

    /// <summary>
    /// 브레이크 시간 완료 후 다음 상태로 전환
    /// </summary>
    private void TransitionToNextState()
    {
        if (_hasDoubleTapped)
        {
            // 더블탭이 감지되었다면 런 상태로 전환
            _playerFSM.ChangeState<PlayerRunState>();
        }
        else
        {
            // 더블탭이 없었다면 바닥 체크 후 상태 전환
            TransitionBasedOnGroundState();
        }
    }

    /// <summary>
    /// 바닥 상태에 따른 상태 전환
    /// </summary>
    private void TransitionBasedOnGroundState()
    {
        if (IsGrounded2D())
        {
            _playerFSM.ChangeState<PlayerIdleState>();
        }
        else
        {
            _owner.RPC_SetAnimatorTrigger("Fall");
            _playerFSM.ChangeState<PlayerFallState>();
        }
    }
} 