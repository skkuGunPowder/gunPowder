using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class InstantiateDestroyManager : MonoBehaviourPun
{
    private static InstantiateDestroyManager _instance;
    public static InstantiateDestroyManager Instance => _instance;
    
    private PhotonView _photonView;

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
        
        _photonView = GetComponent<PhotonView>();
    }

    public void RequestInstantiate(string prefabName, Vector3 position, Quaternion rotation)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Create(prefabName, position, rotation);
        }
        else
        {
            _photonView.RPC(nameof(Create), RpcTarget.MasterClient, prefabName, position, rotation);
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
            _photonView.RPC(nameof(Destroy), RpcTarget.MasterClient, viewID);
        }
    }

    [PunRPC]
    public void Destroy(int viewID)
    {
        PhotonView photonView = PhotonView.Find(viewID);
        if (photonView == null)
        {
            Debug.LogWarning($"PhotonView with ID {viewID} not found for destruction");
            return;
        }

        GameObject objectToDelete = photonView.gameObject;
        if (objectToDelete == null) 
        {
            Debug.LogWarning($"GameObject for PhotonView {viewID} is null");
            return;
        }

        // 1) 소유자가 나면 소유자가 파괴
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(objectToDelete);
            return;
        }

        // 2) 소유자가 존재하고 활성이라면, 마스터가 아니라 소유자에게 파괴를 위임
        bool ownerExists = photonView.Owner != null;
        bool ownerActive = ownerExists && photonView.Owner.IsInactive == false;
        if (ownerExists && ownerActive)
        {
            // 소유자에게만 보낸 RPC로 파괴를 요청
            _photonView.RPC(nameof(OwnerDestroy), photonView.Owner, viewID);
            return;
        }

        // 3) 소유자가 없거나 비활성이면, 마스터가 정리
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(objectToDelete);
        }
        else
        {
            Debug.LogWarning($"Cannot destroy GameObject {objectToDelete.name} - not MasterClient or owner. ViewID: {viewID}");
        }
    }

    [PunRPC]
    public void OwnerDestroy(int viewID)
    {
        PhotonView photonView = PhotonView.Find(viewID);
        if (photonView == null)
        {
            return;
        }
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(photonView.gameObject);
        }
    }
        
}
