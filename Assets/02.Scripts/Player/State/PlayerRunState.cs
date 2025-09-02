using RobustFSM.Base;
using UnityEngine;

/// <summary>
/// 플레이어 러닝 상태 클래스
/// 
/// 역할:
/// - 달리기 중 수평 이동 처리 및 코요테 타임 기반 낙하 전환
/// - 입력 해제/반대 입력에 따른 Idle/Break 전환
/// - 러닝 중 폭탄 공격 처리(직선 투척 + 반동 상태)
/// 
/// 동작 방식:
/// 1. 진입 시 속도/플래그/애니메이션 설정
/// 2. 매 프레임 코요테/지면 체크 → 낙하 전환 판단
/// 3. 입력에 따라 이동/브레이크/아이들 전환
/// 4. 공격 입력 시 즉시 투척 후 Recoil 전환
/// </summary>
public class PlayerRunState : PlayerBaseState
{
    private float _keyReleaseTimer = 0f;
    private float _keyReleaseThreshold = 0.3f;

    // 코요테 타임 관련
    private float _coyoteTimer = 0f;
    private const float COYOTE_TIME = 0.1f;
    private bool _wasGroundedLastFrame = true;

    /// <summary>
    /// 러닝 상태 진입 설정 (타이머/스탯/애니메이션)
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();
        _keyReleaseTimer = 0f;
        _coyoteTimer = 0f;
        _wasGroundedLastFrame = IsGrounded2D();

        // 플레이어 상태
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.RunSpeed;
        _owner.PlayerStat.IsRunning = true;
        _owner.PlayerStat.IsJumping = false;

        // 애니메이션 재생
        _owner.RPC_SetAnimatorTrigger("Run");
    }

    /// <summary>
    /// 러닝 상태 종료 정리 (애니메이션 리셋)
    /// </summary>
    public override void OnExit()
    {
        base.OnExit();
        _owner.RPC_ResetAnimatorTrigger("Run");
    }

    /// <summary>
    /// 러닝 메인 업데이트: 코요테/지면 → 이동/전이 → 공격
    /// </summary>
    public override void MineUpdate()
    {
        base.MineUpdate();

        if (!HandleCoyoteAndGroundTransition())
        {
            return;
        }

        HandleRunMovementAndTransitions();
        HandleRunAttack();
    }


    /// <summary>
    /// 코요테/지면 체크 및 낙하 전환 판단
    /// </summary>
    private bool HandleCoyoteAndGroundTransition()
    {
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
    /// 입력에 따른 러닝 이동/Break/Idle 전환
    /// </summary>
    private void HandleRunMovementAndTransitions()
    {
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;

        bool sameDirHeld = (InputHandler.GetKey(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == 1)
                        || (InputHandler.GetKey(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == -1);

        bool oppositeDirHeld = (InputHandler.GetKey(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == -1)
                             || (InputHandler.GetKey(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == 1);

        if (sameDirHeld)
        {
            velocity.x = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.MyMoveSpeed;
            _owner.Rigidbody2D.linearVelocity = velocity;
            _keyReleaseTimer = 0f;
            return;
        }

        if (oppositeDirHeld)
        {
            _playerFSM.ChangeState<PlayerBreakState>();
            return;
        }

        _keyReleaseTimer += Time.deltaTime;
        velocity.x = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.MyMoveSpeed;
        _owner.Rigidbody2D.linearVelocity = velocity;
        if (_keyReleaseTimer >= _keyReleaseThreshold)
        {
            _playerFSM.ChangeState<PlayerIdleState>();
        }
    }

    /// <summary>
    /// 러닝 중 공격 처리 (직선 투척 후 Recoil 전환)
    /// </summary>
    private void HandleRunAttack()
    {
        if (InputHandler.GetKeyDown(KeyCode.Z) && CanNormalBomb())
        {
            ThrowStraightNormalBomb();
            _playerFSM.ChangeState<PlayerRecoilState>();
            return;
        }
        if (InputHandler.GetKeyDown(KeyCode.X) && CanSpecialBomb())
        {
            ThrowStraightSpecialBomb();
            _playerFSM.ChangeState<PlayerRecoilState>();
            return;
        }
    }
}