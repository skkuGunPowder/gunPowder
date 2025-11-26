using Photon.Pun;
using UnityEngine;

public class PlayerObserveState : PlayerBaseState
{
    public override void OnEnter()
    {
        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;
        _groundRay2D = _owner.GroundRay2D;
        _owner.gameObject.SetActive(false);

        if (_owner.PhotonView.IsMine)
        {
            Debug.Log("observe");
        }
    }

    public override void OnExit()
    {
    }

    public override void MineUpdate()
    {

    }
}
