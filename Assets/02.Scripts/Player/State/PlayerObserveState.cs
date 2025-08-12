using Photon.Pun;
using UnityEngine;

public class PlayerObserveState : PlayerBaseState
{
    public override void OnEnter()
    {
        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;
        _groundRay2D = _owner.GroundRay2D;
        _cameraController = Camera.main.GetComponent<CameraController>();
        _owner.Observe();
    }

    public override void OnExit()
    {
    }

    public override void MineUpdate()
    {
        if (InputHandler.GetKeyDown(KeyCode.LeftArrow))
        {
            _cameraController.SelectTarget(-1);
        }

        if (InputHandler.GetKeyDown(KeyCode.RightArrow))
        {
            _cameraController.SelectTarget(1);
        }
        if(InputHandler.GetKeyDown(KeyCode.Q))
        {
            if (_owner.PhotonView.IsMine)
            {
                PhotonNetwork.Destroy(_owner.gameObject);
            }
        }
    }
}
