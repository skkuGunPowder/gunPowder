using Photon.Pun;
using UnityEngine;

public class PlayerObserveState : PlayerBaseState
{
    public override void OnEnter()
    {
        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;
        _groundRay2D = _owner.GroundRay2D;
        _owner.Observe();
        this.gameObject.SetActive(false);
    }

    public override void OnExit()
    {
    }

    public override void MineUpdate()
    {

    }
}
