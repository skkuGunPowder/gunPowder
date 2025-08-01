using RobustFSM.Base;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    private bool _firstEnter = false;
    
    // 방향 변경 감지용
    private int _lastFacingDirection = 0;

    private const float MAX_FALL_SPEED = -20f; // 최대 낙하 속도
    private const float MAX_JUMP_SPEED = 30f;  // 최대 점프 속도

    public override void OnEnter()
    {
        base.OnEnter();

        // 플레이어 상태
        _owner.PlayerStat.IsRunning = false;
        _owner.PlayerStat.IsJumping = false;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;  // 기본 이동속도
        _owner.PlayerStat.JumpCount = 0;
        _owner.PlayerStat.ResetJumpDashCount();

        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        velocity.x = 0f;
        _owner.Rigidbody2D.linearVelocity = velocity;

        // 애니메이션 재생
        if(_firstEnter)
        {
            _owner.RPC_SetAnimatorTrigger("Idle");
        }
        _firstEnter = true;
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
        IdleMove();
        
        IdleAttack();
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
        if (Input.GetKeyDown(KeyCode.Z) && CanNormalBomb())
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                if (Input.GetKeyDown(KeyCode.Z) && CanNormalBomb())
                {
                    ThrowNormalBomb(EBombSpawnPoint.Up);
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                if (Input.GetKeyDown(KeyCode.Z) && CanNormalBomb())
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
        if (Input.GetKeyDown(KeyCode.X) && CanSpecialBomb())
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                if (Input.GetKeyDown(KeyCode.X) && CanSpecialBomb())
                {
                    ThrowSpecialBomb(EBombSpawnPoint.Up);
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                if (Input.GetKeyDown(KeyCode.X) && CanSpecialBomb())
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
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)
        || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            int newDirection = Input.GetKey(KeyCode.LeftArrow) ? -1 : 1;
            /*
            // 방향이 바뀔 때만 RPC 호출
            if (_lastFacingDirection != newDirection)
            {
                _owner.RPC_SetFacingDirection(newDirection);
                _lastFacingDirection = newDirection;
            }*/
            _playerFSM.ChangeState<PlayerWalkState>();
        }
    }

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
}
