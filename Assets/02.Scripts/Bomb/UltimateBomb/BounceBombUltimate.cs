using Photon.Pun;
using UnityEngine;
using DG.Tweening;

public class BounceBombUltimate : Ultimate
{
    [SerializeField] GameObject UltimateBouncePrefab;

    private Vector3 _fireDirection;

    public override void Init()
    {
        base.Init();

        _ownerBombID = "BO0011";
        _fireDirection = transform.right;
    }

    public override void ExcuteUltimate()
    {
        if (_owner.PlayerStat.FacingDirection != 1)
        {
            _fireDirection *= -1;
        }

        _owner.transform.DOShakePosition(1f, 0.1f, 50, 30, false, false).OnComplete(()=>
        {
            GameObject bounceBombOBJ = PhotonNetwork.Instantiate(UltimateBouncePrefab.name, _owner.transform.position, Quaternion.identity);
            UltimateBounceBomb bounceBomb = bounceBombOBJ.GetComponent<UltimateBounceBomb>();
            bounceBomb.PhotonView.RPC(nameof(bounceBomb.SetOwner), RpcTarget.All, _owner.PhotonView.ViewID);
            bounceBomb.PhotonView.RPC(nameof(bounceBomb.ThrowBomb), RpcTarget.All, _fireDirection, _owner.transform.up, _owner.transform.forward);    
        });
    }
}
