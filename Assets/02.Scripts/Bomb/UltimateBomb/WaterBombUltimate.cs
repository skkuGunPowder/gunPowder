using Photon.Pun;
using UnityEngine;

public class WaterBombUltimate : Ultimate
{
    [SerializeField] private GameObject FireTruckPrefab;

    public override void Init()
    {
        base.Init();

        _ownerBombID = "BO0007";
    }

    public override void ExcuteUltimate()
    {
        GameObject fireTruckObj = PhotonNetwork.Instantiate(FireTruckPrefab.name, transform.position, Quaternion.identity);
        FireTruck fireTruck = fireTruckObj.GetComponent<FireTruck>();
        fireTruck.SetPlayer(_owner);
        fireTruck.Summon();
    }
}
