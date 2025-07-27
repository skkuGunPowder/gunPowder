using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class InstantiateDestroyManager : MonoBehaviourPun
{
    private static InstantiateDestroyManager _instance;
    public static InstantiateDestroyManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RequestInstantiate(string prefabName, Vector3 position, Quaternion rotation)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Create(prefabName, position, rotation);
        }
        else
        {
            photonView.RPC(nameof(Create), RpcTarget.MasterClient, prefabName, position, rotation);
        }
    }

    [PunRPC]
    public void Create(string prefabName, Vector3 position, Quaternion rotation)
    {
        PhotonNetwork.Instantiate(prefabName, position, rotation);
    }

    public void RequestDestroy(int viewID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Destroy(viewID);
        }
        else
        {
            photonView.RPC(nameof(Destroy), RpcTarget.MasterClient, viewID);
        }
    }

    [PunRPC]
    public void Destroy(int viewID)
    {
        PhotonView photonView = PhotonView.Find(viewID);
        if (photonView == null)
        {
            return;
        }

        GameObject objectToDelete = photonView.gameObject;
        if (objectToDelete == null) return;

        PhotonNetwork.Destroy(objectToDelete);
    }
        
}
