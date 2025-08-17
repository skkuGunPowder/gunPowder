using Photon.Pun;
using UnityEngine;

public class AirDropJet : Jet
{
    [SerializeField] private Transform _dropTransfrom;
    [SerializeField] private ParticleSystem _dropWarningVFX;
 
    private PhotonView _photonView;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    protected override void DropItem()
    {
        if (!_photonView.IsMine)
        {
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 40f,LayerMask.NameToLayer("Platform"));
        if (hit.collider == null)
        {
            return;
        }

        PhotonNetwork.Instantiate(_dropWarningVFX.name, hit.point, Quaternion.identity);
        PhotonNetwork.Instantiate(_dropItemList[0].name, _dropTransfrom.position, Quaternion.identity);
    }
}
