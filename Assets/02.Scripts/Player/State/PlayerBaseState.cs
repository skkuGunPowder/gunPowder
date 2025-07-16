using RobustFSM.Base;
using UnityEngine;

public class PlayerBaseState : MonoState
{
    protected PlayerFSM _playerFSM;
    protected Player _owner;

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
        if(Input.GetKeyDown(KeyCode.Space))
        {
            _playerFSM.ChangeState<PlayerJumpState>();
        }
    }
}
