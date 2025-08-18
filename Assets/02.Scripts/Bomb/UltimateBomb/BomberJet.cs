using Photon.Pun;
using UnityEngine;

public class BomberJet : Jet
{
    [SerializeField] private Transform _dropTransfrom;

    private PhotonView _photonView;

    private Player _player;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    protected override void DropItem()
    {
        if (_photonView.IsMine)
        {
            GameObject dropItem = PhotonNetwork.Instantiate(_dropItemList[0].name, _dropTransfrom.position, Quaternion.identity);
            UltimateMissileBomb missile = dropItem.GetComponent<UltimateMissileBomb>();
            missile.PhotonView.RPC(nameof(missile.SetOwner), RpcTarget.All, _player.PhotonView.ViewID);
            missile.PhotonView.RPC(nameof(missile.ThrowBomb), RpcTarget.All, _dropTransfrom.right, _dropTransfrom.up, _dropTransfrom.forward);
        }
    }
}
