using Photon.Pun;
using UnityEngine;

public class MortarUltimate : Ultimate
{
    private const string ID = "BO0010";

    [Header("References")]
    [SerializeField] GameObject UltimateMortarPrefab;

    public override void Init()
    {
        _ownerBombID = "BO0009";

        _bombStat = ItemDatabase.Instance.GetStat<BombStat>(ID);
    }

    public override void ExcuteUltimate()
    {
        if (_owner.PhotonView.IsMine)
        {
            GameObject mortarOBJ = PhotonNetwork.Instantiate(UltimateMortarPrefab.name, _owner.transform.position, Quaternion.identity);
            Mortar mortar = mortarOBJ.GetComponent<Mortar>();
            mortar.PhotonView.RPC(nameof(mortar.SetOwner), RpcTarget.All, _owner.PhotonView.ViewID);
        }
    }
}
