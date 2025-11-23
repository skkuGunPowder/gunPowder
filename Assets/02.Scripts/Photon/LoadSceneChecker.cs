using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class LoadSceneChecker : MonoBehaviourPunCallbacks
{
    private PhotonView _photonView;
    
    public event Action<int, bool> OnLoading;
    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        EventManager.Instance.OnPlayerChanged += PlayerLoadCheck;
    }

    private void Update()
    {
        PhotonNetwork.NetworkingClient.Service();
    }

    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer ,Hashtable changedProps)
    {
        if (changedProps.ContainsKey(EProperties.IsLoad.ToString()) && changedProps[EProperties.IsLoad.ToString()] != null)
        {
            OnLoading?.Invoke(targetPlayer.ActorNumber, (bool)changedProps[EProperties.IsLoad.ToString()]);
            
            if (PhotonNetwork.IsMasterClient == false)
            {
                return;
            }

            PlayerLoadCheck();

        }
    }

    private void PlayerLoadCheck(PhotonPlayer player = null)
    {
        if (IsLoadEnd())
        {
            _photonView.RPC(nameof(Rpc_LoadEnd), RpcTarget.All);
        }
    }
    
    private bool IsLoadEnd(PhotonPlayer player = null)
    {
        List<PhotonPlayer> playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
        
        foreach (PhotonPlayer p in playerList)
        {
            bool isLoaded = p.CustomProperties.ContainsKey(EProperties.IsLoad.ToString()) && (bool)p.CustomProperties[EProperties.IsLoad.ToString()];
           
            if (isLoaded == false)
            {
                return false ;
            }
        }
        return true;
    }

    [PunRPC]
    private void Rpc_LoadEnd()
    {
        EventManager.Instance.LoadEnd();
    }

}
