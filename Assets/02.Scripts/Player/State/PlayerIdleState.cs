using RobustFSM.Base;
using UnityEngine;

/// <summary>
/// 플레이어 대기(Idle) 상태 클래스
/// 
/// 역할:
/// - 서 있는 동안의 기본 상태 유지
/// - 방향 입력 시 걷기 상태 전환
/// - 대기 중 일반/특수 폭탄 공격 처리
/// - 점프 착지 직후의 애니메이션 전환 처리
/// 
/// 동작 방식:
/// 1) 상태 진입 시 플레이어 기본 스탯 초기화 및 마찰 적용(젖지 않았을 때만)
/// 2) 점프 상태에서 넘어온 경우 착지 애니메이션 완료까지 대기 후 Idle 트리거
/// 3) 방향 입력 감지 시 Walk 상태로 전환
/// 4) 공격 입력(Z/X)에 따른 폭탄 배치/투척 처리
/// </summary>
public class PlayerIdleState : PlayerBaseState
{
    private bool _firstEnter = false;
    
    // 방향 변경 감지용
    private int _lastFacingDirection = 0;
    
    private const float MAX_FALL_SPEED = -20f; // 최대 낙하 속도
    private const float MAX_JUMP_SPEED = 30f;  // 최대 점프 속도

    // 착지 처리 개선
    private bool _isFromJumpState = false;
    private float _landingAnimationTimer = 0f;
    private const float LANDING_ANIMATION_DURATION = 0.3f;
    
    // 착지 플래그 추가
    private static bool _landingFromJump = false;

    public override void OnEnter()
    {
        base.OnEnter();
        
        // 진입 초기화 (플래그/스탯/마찰/애니메이션)
        InitializeOnEnter();
        _firstEnter = true;
    }

    /// <summary>
    /// 점프 상태에서 착지할 때 호출되는 정적 메서드
    /// </summary>
    public static void SetLandingFromJump()
    {
        _landingFromJump = true;
    }

    public override void OnExit()
    {
        base.OnExit();
        _owner.RPC_ResetAnimatorTrigger("Land");
        _owner.RPC_ResetAnimatorTrigger("Idle");
    }

    /// <summary>
    /// 실제 행동 로직
    /// </summary>
    public override void MineUpdate()
    {
        base.MineUpdate();
        
        // 애니메이션/입력 처리
        HandleLandingAnimation();
        HandleIdleMovement();
        HandleIdleAttack();
    }

    /// <summary>
    /// 착지 애니메이션 처리
    /// </summary>
    private void HandleLandingAnimation()
    {
        if (_isFromJumpState)
        {
            _landingAnimationTimer += Time.deltaTime;
            
            // 착지 애니메이션 완료 후 Idle 애니메이션으로 전환
            if (_landingAnimationTimer >= LANDING_ANIMATION_DURATION)
            {
                _owner.RPC_SetAnimatorTrigger("Idle");
                _isFromJumpState = false;
            }
        }
    }

    /// <summary>
    /// Idle 상태(방향키 입력이 없고 가만히 있는 상태) 에서는 
    /// 폭탄 두기 공격이 나간다. 
    /// 방향키 입력이 없기 때문에 캐릭터의 Right(정면) 으로 공격이 발생한다.
    /// </summary>
    private void IdleAttack()
    {
        IdleNormalAttack();
        IdleSpecialAttack();
    }

    /// <summary>
    /// Idle 공격 처리 래퍼 -> 내부 처리 위임
    /// </summary>
    private void HandleIdleAttack()
    {
        ProcessIdleAttack();
    }

    /// <summary>
    /// Idle 공격 처리 본체
    /// </summary>
    private void ProcessIdleAttack()
    {
        IdleAttack();
    }

    private void IdleNormalAttack()
    {
        if (InputHandler.GetKeyDown(KeyCode.Z) && CanNormalBomb())
        {
            if (InputHandler.GetKey(KeyCode.UpArrow))
            {
                if (InputHandler.GetKeyDown(KeyCode.Z) && CanNormalBomb())
                {
                    ThrowNormalBomb(EBombSpawnPoint.Up);
                }
            }
            else if (InputHandler.GetKey(KeyCode.DownArrow))
            {
                if (InputHandler.GetKeyDown(KeyCode.Z) && CanNormalBomb())
                {
                    ThrowNormalBomb(EBombSpawnPoint.Down);
                }
            }
            else
            {
                // 보고 있는 방향으로 폭탄을 둔다.
                if (_owner.PlayerStat.FacingDirection == 1)
                {
                    PlaceNormalBomb(EBombSpawnPoint.Right);
                }
                else
                {
                    PlaceNormalBomb(EBombSpawnPoint.Left);
                }
            }
        }
    }

    private void IdleSpecialAttack()
    {
        if (InputHandler.GetKeyDown(KeyCode.X) && CanSpecialBomb())
        {
            if (InputHandler.GetKey(KeyCode.UpArrow))
            {
                if (InputHandler.GetKeyDown(KeyCode.X) && CanSpecialBomb())
                {
                    ThrowSpecialBomb(EBombSpawnPoint.Up);
                }
            }
            else if (InputHandler.GetKey(KeyCode.DownArrow))
            {
                if (InputHandler.GetKeyDown(KeyCode.X) && CanSpecialBomb())
                {
                    ThrowSpecialBomb(EBombSpawnPoint.Down);
                }
            }
            else
            {
                // 보고 있는 방향으로 폭탄을 둔다.
                // FacingDirection 은 1 이면 오른쪽, -1 이면 왼쪽
                if (_owner.PlayerStat.FacingDirection == 1)
                {
                    PlaceSpecialBomb(EBombSpawnPoint.Right);
                }
                else
                {
                    PlaceSpecialBomb(EBombSpawnPoint.Left);
                }
            }
        }
    }

    /// <summary>
    /// 방향키 입력이 발생하면 걷기 상태로 전환
    /// </summary>
    private void IdleMove()
    {
        // 이동키를 받으면 걷기 상태로 전환
        if (InputHandler.GetKeyDown(KeyCode.LeftArrow) || InputHandler.GetKeyDown(KeyCode.RightArrow)
        || InputHandler.GetKey(KeyCode.LeftArrow) || InputHandler.GetKey(KeyCode.RightArrow))
        {
            _owner.RPC_SetFacingDirection(InputHandler.GetKey(KeyCode.LeftArrow) ? -1 : 1);
            _playerFSM.ChangeState<PlayerWalkState>();
        }
    }

    /// <summary>
    /// Idle 이동 처리 래퍼
    /// </summary>
    private void HandleIdleMovement()
    {
        IdleMove();
    }

    /// <summary>
    /// Idle 상태 진입 시 초기화 처리
    /// </summary>
    private void InitializeOnEnter()
    {
        InitializeLandingFlag();
        ResetPlayerStateForIdle();
        ApplyGroundFrictionIfNotWet();
        SetupEnterAnimation();
    }

    /// <summary>
    /// 점프 착지 플래그 처리
    /// </summary>
    private void InitializeLandingFlag()
    {
        _isFromJumpState = _landingFromJump;
        _landingFromJump = false;
    }

    /// <summary>
    /// Idle에 맞게 플레이어 스탯 리셋
    /// </summary>
    private void ResetPlayerStateForIdle()
    {
        _owner.PlayerStat.IsRunning = false;
        _owner.PlayerStat.IsJumping = false;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
        _owner.PlayerStat.JumpCount = 0;
        _owner.PlayerStat.IsDownJump = false;
        _owner.PlayerStat.ResetJumpDashCount();
    }

    /// <summary>
    /// 젖지 않았을 때만 수평 속도 0으로 고정(지면 마찰)
    /// </summary>
    private void ApplyGroundFrictionIfNotWet()
    {
        if(!_owner.PlayerStat.IsWet)
        {
            Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
            velocity.x = 0f;
            _owner.Rigidbody2D.linearVelocity = velocity;
        }
    }

    /// <summary>
    /// 상태 진입 시 애니메이션 세팅
    /// </summary>
    private void SetupEnterAnimation()
    {
        if (_isFromJumpState)
        {
            _landingAnimationTimer = 0f;
            // 착지 애니메이션은 이미 JumpState에서 트리거됨
        }
        else
        {
            if(_firstEnter)
            {
                _owner.RPC_SetAnimatorTrigger("Idle");
            }
        }
    }
}
