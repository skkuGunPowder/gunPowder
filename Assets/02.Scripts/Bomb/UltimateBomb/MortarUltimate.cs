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
            UltimateMortar mortar = mortarOBJ.GetComponent<UltimateMortar>();
            mortar.PhotonView.RPC(nameof(mortar.SetOwner), RpcTarget.All, _owner.PhotonView.ViewID);
            mortar.PhotonView.RPC(nameof(mortar.PlaceBomb), RpcTarget.All, Vector3.zero, Vector3.zero, Vector3.zero);
        }
    }
}
