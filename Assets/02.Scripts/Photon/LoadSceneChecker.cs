using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class LoadSceneChecker : MonoBehaviourPunCallbacks
{
    private bool _isLoad = false;
    private PhotonView _photonView;
    
    public event Action<int, bool> OnLoading;
    public event Action OnLoadEnd;
    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }
    
    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer ,Hashtable changedProps)
    {
        
        if (changedProps.ContainsKey(EProperties.IsLoad.ToString()) && changedProps[EProperties.IsLoad.ToString()] != null)
        {
            Debug.Log(targetPlayer + "로딩 체크하기");
            OnLoading?.Invoke(targetPlayer.ActorNumber, (bool)changedProps[EProperties.IsLoad.ToString()]);
            
            if (PhotonNetwork.IsMasterClient == false)
            {
                return;
            }

            if (PlayerLoadCheck())
            {
                _photonView.RPC(nameof(Rpc_LoadEnd), RpcTarget.All);
            }
        }
    }
    
    private bool PlayerLoadCheck()
    {
        List<PhotonPlayer> playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
        
        foreach (PhotonPlayer p in playerList)
        {
            bool isLoaded = p.CustomProperties.ContainsKey(EProperties.IsLoad.ToString()) && (bool)p.CustomProperties[EProperties.IsLoad.ToString()];
            Debug.Log($"Player {p.NickName}{p.ActorNumber} - SceneLoaded: {isLoaded}");
            if (isLoaded == false)
            {
                Debug.Log($"Player {p.NickName}{p.ActorNumber} - SceneLoaded: {isLoaded}");
                return false ;
            }
        }
       
        Debug.Log("로드 완료");
        return true;
    }

    [PunRPC]
    private void Rpc_LoadEnd()
    {
        OnLoadEnd?.Invoke();
    }

    private void SetLoadState(bool isLoad)
    {
        Hashtable load = new Hashtable()
        {
            { EProperties.IsLoad.ToString(), isLoad }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(load);
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + $"{PhotonNetwork.LocalPlayer.CustomProperties[EProperties.IsLoad.ToString()]}");
    }

}
