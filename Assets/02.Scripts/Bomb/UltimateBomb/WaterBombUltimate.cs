using Photon.Pun;
using UnityEngine;

public class WaterBombUltimate : Ultimate
{
    [SerializeField] private GameObject FireTruckPrefab;
    private GameObject _fireTruckObj;

    public override void Init()
    {
        base.Init();

        _ownerBombID = "BO0007";
    }

    public override void ExcuteUltimate()
    {
        bool isFacingRight = false;
        if (_owner.PlayerStat.FacingDirection == 1)
        {
            _fireTruckObj = PhotonNetwork.Instantiate(FireTruckPrefab.name, transform.position, Quaternion.Euler(0, 180, 0));
            isFacingRight = true;
        }
        else
        {
            _fireTruckObj = PhotonNetwork.Instantiate(FireTruckPrefab.name, transform.position, Quaternion.identity);
            isFacingRight = false;
        }

        FireTruck fireTruck = _fireTruckObj.GetComponent<FireTruck>();
        fireTruck.PhotonView.RPC(nameof(fireTruck.Launch), RpcTarget.All, _owner.PhotonView.ViewID, isFacingRight);
    }
}
