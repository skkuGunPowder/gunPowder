using Photon.Pun;
using UnityEngine;

public class SuicideUltimate : Ultimate
{
    private const string ID = "BO0019";

    [Header("References")]
    [SerializeField] GameObject UltimateSuicidePrefab;

    public override void Init()
    {
        _ownerBombID = "BO0018";

        _bombStat = ItemDatabase.Instance.GetStat<BombStat>(ID);
    }

    public override void ExcuteUltimate()
    {
        if (_owner.PhotonView.IsMine)
        {
            GameObject suicideBombOBJ = PhotonNetwork.Instantiate(UltimateSuicidePrefab.name, _owner.transform.position, Quaternion.identity);
            UltimateSuicide suicideBomb = suicideBombOBJ.GetComponent<UltimateSuicide>();
            suicideBomb.PhotonView.RPC(nameof(suicideBomb.SetOwner), RpcTarget.All, _owner.PhotonView.ViewID);
            suicideBomb.PhotonView.RPC(nameof(suicideBomb.PlaceBomb), RpcTarget.All, Vector3.zero, Vector3.zero, Vector3.zero);
        }
    }
}
