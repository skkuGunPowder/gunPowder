using Photon.Pun;
using UnityEngine;

public class MissileUltimate : Ultimate
{
    [SerializeField] private GameObject BomberJetPrefab;
    [SerializeField] private GameObject MissileUitimateBombPrefab;

    public override void Init()
    {
        base.Init();

        _ownerBombID = "BO0005";
    }

    public override void ExcuteUltimate()
    {
        GameObject bomberjet = PhotonNetwork.Instantiate(BomberJetPrefab.name, transform.position, Quaternion.identity);
        bomberjet.GetComponent<BomberJet>().SetPlayer(_owner);
    }
}