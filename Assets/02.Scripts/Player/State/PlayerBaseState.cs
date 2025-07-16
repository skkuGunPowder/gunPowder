using RobustFSM.Base;
using UnityEngine;

public class PlayerBaseState : MonoState
{
    protected PlayerFSM _playerFSM;
    protected Player _owner;

    private float _attackTimer = 0f;
    private float _lastNormalBombTime = 0f;
    private float _lastSpecialBombTime = 0f;

    // 코요테 타임 관련
    private float _coyoteTimer = 0f;
    private const float COYOTE_TIME = 0.15f;
    private bool _wasGroundedLastFrame = true;

    public override void OnEnter()
    {
        base.OnEnter();
        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;
        Debug.Log($"Enter {this.GetType().Name} State");
        _coyoteTimer = 0f;
        _wasGroundedLastFrame = IsGrounded();
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log($"Exit {this.GetType().Name} State");
    }

    public virtual void Update()
    {
        _attackTimer += Time.deltaTime;

        // 바닥 체크 및 코요테 타임 관리
        bool isGrounded = IsGrounded();
        if (isGrounded)
        {
            _coyoteTimer = 0f;
        }
        else
        {
            _coyoteTimer += Time.deltaTime;
        }

        // 바닥에서 떨어진 순간(이전 프레임엔 있었고, 이번 프레임엔 없음)
        if (_wasGroundedLastFrame && !isGrounded)
        {
            if (!_playerFSM.IsCurrentState<PlayerJumpState>() &&
                !_playerFSM.IsCurrentState<PlayerJumpDashState>() &&
                !_playerFSM.IsCurrentState<PlayerDashState>())
            {
                _playerFSM.ChangeState<PlayerJumpState>();
            }
        }
        _wasGroundedLastFrame = isGrounded;

        JumpInput();
    }

    // 하위에서 사용하고 싶은 것만 사용한다.
    protected virtual void JumpInput()
    {
        // 대쉬 중에는 점프를 하지 못한다.
        if (_playerFSM.IsCurrentState<PlayerDashState>() || _playerFSM.IsCurrentState<PlayerJumpDashState>())
            return;

        // 코요테 타임 내에는 점프 허용
        bool isGrounded = IsGrounded();
        if (!isGrounded && _coyoteTimer > COYOTE_TIME)
            return;

        if (Input.GetKeyDown(KeyCode.Space) && _owner.PlayerStat.CanJump())
        {
            //_owner.PlayerStat.IncrementJumpCount();
            _playerFSM.ChangeState<PlayerJumpState>();
        }
    }

    // Raycast로 바닥 체크
    protected virtual bool IsGrounded()
    {
        float rayDistance = 0.2f;
        Vector3 origin = _owner.transform.position;
        var cc = _owner.GetComponent<CharacterController>();
        if (cc != null)
        {
            rayDistance = cc.height / 2f + 0.1f;
        }
        return Physics.Raycast(origin, Vector3.down, rayDistance);
    }

    protected virtual bool CanNormalBomb()
    {
        if(_attackTimer - _lastNormalBombTime < _owner.NormalBomb.BombCoolTime)
        {
            return false;
        }
        return true;
    }

    protected virtual bool CanSpecialBomb()
    {
        if(_attackTimer - _lastSpecialBombTime < _owner.SpecialBomb.BombCoolTime)
        {
            return false;
        }
        return true;
    }

    protected virtual void SetLastNormalBombTime()
    {
        _lastNormalBombTime = _attackTimer;
    }

    protected virtual void SetLastSpecialBombTime()
    {
        _lastSpecialBombTime = _attackTimer;
    }
}
