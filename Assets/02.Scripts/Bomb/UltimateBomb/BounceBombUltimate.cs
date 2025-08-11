using Photon.Pun;
using UnityEngine;

public class BounceBombUltimate : Ultimate
{
    [SerializeField] GameObject UltimateBouncePrefab;

    public override void Init()
    {
        base.Init();

        _ownerBombID = "BO0011";
    }

    public override void ExcuteUltimate()
    {
        GameObject bounceBombOBJ = PhotonNetwork.Instantiate(UltimateBouncePrefab.name, _owner.transform.position, Quaternion.identity);
        UltimateBounceBomb bounceBomb = bounceBombOBJ.GetComponent<UltimateBounceBomb>();
        bounceBomb.PhotonView.RPC(nameof(bounceBomb.SetOwner), RpcTarget.All, _owner.PhotonView.ViewID);
        bounceBomb.PhotonView.RPC(nameof(bounceBomb.ThrowBomb), RpcTarget.All, _owner.transform.right, _owner.transform.up, _owner.transform.forward);
    }
}
