using RobustFSM.Base;
using UnityEngine;

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
        
        // 착지 플래그 확인 (더 안전한 방법)
        _isFromJumpState = _landingFromJump;
        _landingFromJump = false; // 플래그 리셋

        // 플레이어 상태
        _owner.PlayerStat.IsRunning = false;
        _owner.PlayerStat.IsJumping = false;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;  // 기본 이동속도
        _owner.PlayerStat.JumpCount = 0;
        _owner.PlayerStat.IsDownJump = false;
        _owner.PlayerStat.ResetJumpDashCount();

        // 플레이어가 젖으면 미끄러진다.
        if(!_owner.PlayerStat.IsWet)
        {
            Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
            velocity.x = 0f;
            _owner.Rigidbody2D.linearVelocity = velocity;
        }

        // 착지 애니메이션 처리
        if (_isFromJumpState)
        {
            _landingAnimationTimer = 0f;
            // 착지 애니메이션은 이미 JumpState에서 트리거됨
        }
        else
        {
            // 애니메이션 재생
            if(_firstEnter)
            {
                _owner.RPC_SetAnimatorTrigger("Idle");
            }
        }
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
        
        // 착지 애니메이션 처리
        HandleLandingAnimation();
        
        IdleMove();
        IdleAttack();
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
}
