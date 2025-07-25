using Photon.Pun;
using UnityEngine;

public class PhotonPoolManager : Singleton<PhotonPoolManager>
{
    public PhotonPool PhotonPool;

    private void Awake()
    {
    }

    public void RequestDelete(int viewID)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonPool.Destroy(PhotonView.Find(viewID).gameObject);
        }
    }
}
