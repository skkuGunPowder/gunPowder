using RobustFSM.Base;
using UnityEngine;

public class PlayerBaseState : MonoState
{
    protected PlayerFSM _playerFSM;
    protected Player _owner;

    private float _attackTimer = 0f;
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
        // 공격 쿨타임
        _attackTimer += Time.deltaTime;

        JumpInput();
    }

    // 하위에서 사용하고 싶은 것만 사용한다.
    protected virtual void JumpInput()
    {
        // 대쉬 중에는 점프를 하지못한다.
        if(_playerFSM.IsCurrentState<PlayerDashState>() || _playerFSM.IsCurrentState<PlayerJumpDashState>())
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            _playerFSM.ChangeState<PlayerJumpState>();
        }
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
