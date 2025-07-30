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

        // 권한 체크: MasterClient이거나 소유자인 경우에만 제거
        if (PhotonNetwork.IsMasterClient || photonView.IsMine)
        {
            PhotonNetwork.Destroy(objectToDelete);
        }
        else
        {
            Debug.LogWarning($"Cannot destroy GameObject {objectToDelete.name} - not MasterClient or owner. ViewID: {viewID}");
        }
    }
        
}
