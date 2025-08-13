using UnityEngine;
using RobustFSM.Base;
using System.Collections;

public class PlayerJumpDashState : PlayerBaseState
{
    private float _dashTimer = 0f;
    private float _yVelocity = 0f;
    private float _xVelocity = 0f;
    private float _gravity = -40f;
    private float _originalGravityScale;

    // 착지 감지 개선
    private bool _wasGroundedLastFrame = false;
    private float _airborneTimer = 0f;
    private const float MIN_AIRBORNE_TIME = 0.05f;
    private const float LANDING_CHECK_DELAY = 0.1f;
    private float _landingCheckTimer = 0f;
    private bool _isLanding = false;
    private bool _landingConfirmed = false;

    private float _jumpDashEffectOffTime = 0.4f;

    public override void OnEnter()
    {
        base.OnEnter();
        // 플레이어 상태
        _owner.PlayerStat.IsRunning = true;
        _owner.PlayerStat.IsJumping = true;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.DashSpeed;
        _owner.PlayerStat.IncrementJumpDashCount();
        _originalGravityScale = _owner.Rigidbody2D.gravityScale;
        _owner.Rigidbody2D.gravityScale = 0f;
        
        _yVelocity = 0f;
        _xVelocity = 0f;
        _dashTimer = 0f;

        // 착지 감지 변수 초기화
        _isLanding = false;
        _landingConfirmed = false;
        _landingCheckTimer = 0f;
        _groundRay2D.Cast();
        _wasGroundedLastFrame = _groundRay2D.Performed;
        _airborneTimer = 0f;

        _owner.RPC_SetAnimatorTrigger("JumpDash");
        _owner.RPC_JumpDashEffect(true);
        _owner.RPC_SetGhostTrail(true);
    }

    public override void OnExit()
    {
        base.OnExit();
        _owner.Rigidbody2D.gravityScale = _originalGravityScale;
        _owner.RPC_ResetAnimatorTrigger("JumpDash");
        StartCoroutine(JumpDashEffectOffCoroutine());
       
    }

    private IEnumerator JumpDashEffectOffCoroutine()
    {
        yield return new WaitForSeconds(_jumpDashEffectOffTime);
         _owner.RPC_SetGhostTrail(false);
    }

    /// <summary>
    /// 실제 행동 로직
    /// </summary>
    public override void MineUpdate()
    {
        // 착지 감지 개선
        HandleLandingDetection();

        // 착지 확인되면 상태 전환
        if (_landingConfirmed)
        {
            // DamagedState에서 온 경우가 아니라면 착지 플래그 설정
            if (!_playerFSM.IsPreviousState<PlayerDamagedState>())
            {
                PlayerIdleState.SetLandingFromJump(); // 착지 플래그 설정
            }
            SyncStateChange<PlayerIdleState>();
            return;
        }

        // 대쉬 시간 종료 후 바닥 체크
        if(_dashTimer >= _owner.PlayerStat.DashTime)
        {
            
            // 바닥에 있는지 체크
            if (IsGrounded2D())
            {
                _playerFSM.ChangeState<PlayerIdleState>();
            }
            else
            {
                _playerFSM.ChangeState<PlayerFallState>();
            }
            return;
        }
        else
        {
            // 대쉬 이동후 낙하
            _dashTimer += Time.deltaTime;
            Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
            velocity.x = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.MyMoveSpeed;
            velocity.y = 0;
            _owner.Rigidbody2D.linearVelocity = velocity;
        }
    }

    /// <summary>
    /// 착지 감지 로직 개선
    /// </summary>
    private void HandleLandingDetection()
    {
        // 레이캐스트는 로컬 플레이어에서만 실행
        if (!_owner.PhotonView.IsMine)
        {
            return;
        }

        _groundRay2D.Cast();
        bool isGroundedNow = _groundRay2D.Performed;
        
        // 공중 시간 계산
        if (!isGroundedNow)
        {
            _airborneTimer += Time.deltaTime;
        }
        
        // 착지 감지 (이전에 공중이었다가 지금 땅에 닿음)
        bool isLandingSoon = !_wasGroundedLastFrame && isGroundedNow && _airborneTimer > MIN_AIRBORNE_TIME;
        
        if (isLandingSoon)
        {
            // 착지 애니메이션 트리거
            _owner.RPC_SetAnimatorTrigger("Land");
            _isLanding = true;
            _landingCheckTimer = 0f;
        }
        
        // 착지 확인 (일정 시간 후 상태 전환)
        if (_isLanding)
        {
            _landingCheckTimer += Time.deltaTime;
            if (_landingCheckTimer >= LANDING_CHECK_DELAY && isGroundedNow)
            {
                _landingConfirmed = true;
                Debug.Log("[PlayerJumpDashState] Landing confirmed, transitioning to IdleState");
            }
        }
        
        _wasGroundedLastFrame = isGroundedNow;
    }
}
