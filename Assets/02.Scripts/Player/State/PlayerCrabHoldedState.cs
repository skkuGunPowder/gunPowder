using UnityEngine;

public class PlayerCrabHoldedState : PlayerBaseState
{
    public override void OnEnter()
    {
        base.OnEnter();
        _owner.RPC_SetAnimatorTrigger("HitLoop");
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void MineUpdate()
    {

    }
}
