using RobustFSM.Base;
using UnityEngine;

public class PlayerBaseState : MonoState
{
    protected PlayerFSM _playerFSM;
    protected Player _owner;

    private float _lastNormalBombTime = 0f;
    private float _lastSpecialBombTime = 0f;



    public override void OnEnter()
    {
        base.OnEnter();
        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;
        Debug.Log($"Enter {this.GetType().Name} State");
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log($"Exit {this.GetType().Name} State");
    }

    public virtual void Update()
    {
        JumpInput();
    }

    // 하위에서 사용하고 싶은 것만 사용한다.
    protected virtual void JumpInput()
    {
        if (_playerFSM.IsCurrentState<PlayerDashState>() || _playerFSM.IsCurrentState<PlayerJumpDashState>())
            return;

        if (Input.GetKeyDown(KeyCode.Space) && _owner.PlayerStat.CanJump())
        {
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
        if(_owner.AttackTimer - _lastNormalBombTime < _owner.NormalBomb.BombCoolTime)
        {
            return false;
        }
        return true;
    }

    protected virtual bool CanSpecialBomb()
    {
        if(_owner.AttackTimer - _lastSpecialBombTime < _owner.SpecialBomb.BombCoolTime)
        {
            return false;
        }
        return true;
    }

    protected virtual void SetLastNormalBombTime()
    {
        _lastNormalBombTime = _owner.AttackTimer;
    }

    protected virtual void SetLastSpecialBombTime()
    {
        _lastSpecialBombTime = _owner.AttackTimer;
    }
}
