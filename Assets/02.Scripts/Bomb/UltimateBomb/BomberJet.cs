using Photon.Pun;
using UnityEngine;

public class BomberJet : Jet
{
    [SerializeField] private Transform _dropTransfrom;

    private Player _player;

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    protected override void DropItem()
    {
        GameObject dropItem = PhotonNetwork.Instantiate(_dropItemList[0].name, _dropTransfrom.position, Quaternion.identity);
        UltimateMissileBomb missile = dropItem.GetComponent<UltimateMissileBomb>();
        missile.PhotonView.RPC(nameof(missile.SetOwner), RpcTarget.All, _player.PhotonView.ViewID);
        missile.PhotonView.RPC(nameof(missile.ThrowBomb), RpcTarget.All, _dropTransfrom.right, _dropTransfrom.up, _dropTransfrom.forward);
    }
}
