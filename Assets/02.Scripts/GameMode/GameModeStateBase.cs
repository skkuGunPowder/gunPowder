using Photon.Pun;
using UnityEngine;

public abstract class GameModeStateBase :MonoBehaviour
{
    public EModeState State;
    protected GameModeBase _gameMode;
    protected PhotonView _photonView;
    
    public virtual void Initialize(GameModeBase gameMode)
    {
        _gameMode = gameMode;
        _photonView = gameMode.GetComponent<PhotonView>();
    }

    public virtual void Enter() { }
    public virtual void Tick() { }
    public virtual void Exit() { }
}
    