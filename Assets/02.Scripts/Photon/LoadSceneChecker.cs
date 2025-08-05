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
    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }
    
    private void Start()
    {
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
            PlayerLoadCheck();
        }
    }
    
    private void PlayerLoadCheck()
    {
        List<PhotonPlayer> playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
        
        foreach (PhotonPlayer p in playerList)
        {
            bool isLoaded = p.CustomProperties.ContainsKey(EProperties.IsLoad.ToString()) && (bool)p.CustomProperties[EProperties.IsLoad.ToString()];
            Debug.Log($"Player {p.NickName}{p.ActorNumber} - SceneLoaded: {isLoaded}");
            if (isLoaded == false)
            {
                Debug.Log($"Player {p.NickName}{p.ActorNumber} - SceneLoaded: {isLoaded}");
                return ;
            }
        }
       
        Debug.Log("로드 완료");
        EventManager.Instance.LoadFinished();
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
