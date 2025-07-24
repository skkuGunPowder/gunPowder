using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

[RequireComponent(typeof(PhotonView))]
public class LoadSceneChecker : MonoBehaviourPunCallbacks
{
    private bool _isLoad = false;
    private PhotonView _photonView;
    
    public event Action OnLoadFinished; 

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }
    
    private void Start()
    {
        if (PhotonNetwork.InRoom == false)
        {
            return;
        }
        
        SetLoadState(true);
        
    }
    
    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer ,Hashtable changedProps)
    {
        if (changedProps.ContainsKey(EProperties.IsLoad.ToString()) && changedProps[EProperties.IsLoad.ToString()] != null)
        {
            Debug.Log(targetPlayer + "로딩 체크하기");
            PlayerLoadCheck();
        }
    }
    
    private void PlayerLoadCheck()
    {
        // if (PhotonNetwork.IsMasterClient == false)
        // {
        //     return;
        // }
        //
        List<PhotonPlayer> playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
        
        foreach (PhotonPlayer p in playerList)
        {
            bool isLoaded = p.CustomProperties.ContainsKey(EProperties.IsLoad.ToString()) && (bool)p.CustomProperties[EProperties.IsLoad.ToString()];
            Debug.Log($"Player {p.NickName} - SceneLoaded: {isLoaded}");
            if (isLoaded == false)
            {
                Debug.Log($"Player {p.NickName} - SceneLoaded: {isLoaded}");
                return ;
            }
        }
       
        OnLoadFinished?.Invoke();
        
        SetLoadState(false);
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
