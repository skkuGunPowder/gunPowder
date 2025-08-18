using UnityEngine;
using Photon.Pun;

public class AirDropItemGunPowder : AirDropItemBase, IAirDropItem
{
    public int _gunpowderAmount = 5;

    public override void Use()
    {
        Debug.LogWarning($"건파우더 아이템 사용 {_gunpowderAmount}개 습득");
        //_owner.RPC_ReleaseGunPowder(_owner.transform.position, _owner.PhotonView.ViewID, _gunpowderAmount, 30, 1, false);
        object[] data = new object[] {_owner.PhotonView.ViewID, false, 1, 9999};
        for(int i=0; i<_gunpowderAmount; i++)
        {
            PhotonNetwork.Instantiate("GunPowder", _owner.transform.position, Quaternion.identity, 0, data);
        }
    }
}
