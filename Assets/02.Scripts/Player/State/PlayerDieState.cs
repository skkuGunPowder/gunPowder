using Photon.Pun;
using UnityEngine;

public class PlayerDieState : PlayerBaseState
{
    private float _timer = 0f;

    public override void OnEnter()
    {
        base.OnEnter();

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() {{EProperties.IsDead.ToString(), true}});

        // 플레이어가 사망할 떄, 사망 폭발이 발생
        ExplosionPool.Instance.Get(_owner.DieExplosionPrefab.name).Explode(true, transform);
        
        Debug.Log("죽음 폭발 발생");
    }

    public override void OnExit()
    {
        base.OnExit();

        // 플레이어 초기화
        _owner.InitializePlayer();
        
        //부활
        InstantiateDestroyManager.Instance.RequestDestroy(gameObject.GetComponent<PhotonView>().ViewID);
    }

    public override void MineUpdate()
    {   
        _playerFSM.ChangeState<PlayerIdleState>();
    }
}
