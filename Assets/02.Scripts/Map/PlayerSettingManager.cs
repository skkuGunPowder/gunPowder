using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonPlayer = Photon.Realtime.Player;

[RequireComponent(typeof(PhotonView))]
public class PlayerSettingManager : MonoBehaviour
{
    private List<int> _playerList = new List<int>(); //현재 있는 플레이어들
    public List<int> PlayerList => _playerList;
    
    private PhotonView _photonView;
    
    public PlayerSpawner Spawner;
    public LoadSceneChecker LoadChecker;
    
    // 현재 룸 프로퍼티 가져오기 => 플레이어 세팅해주기
    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        Init();
        // EventManager.Instance.OnLoadEnd += Init;
    }

    public void Init()
    {
        Debug.Log("플레이어 세팅매니저 이벤트");
        // 캐릭터 순번 세팅
        SpawnSetting();
    }
    
    [PunRPC]
    private void Rpc_SpawnPlayer(int[] playerList)
    {
        _playerList.Clear();
        _playerList = new List<int>(playerList);
        Debug.Log("RPC로 보내준 리스트의 카운트 :" + _playerList.Count);
        for (int i = 0; i < _playerList.Count; i++)
        {
            Debug.Log($"소환해야하는 플레이어 리스트 {_playerList[i]}");
        }
        SpawnPlayer();
    }
    
    // 프로퍼티 불러오기  => 플레이어 리스트
    private void SpawnSetting()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        _playerList.Clear();
        
        if (PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] != null)
        {
            int[] players = PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] as int[];   
            
            List<int> currentPlayerList = new List<int>(players);
            
            foreach (int actorNumber in currentPlayerList)
            {
                Debug.Log("스폰 세팅 actorNumber " + actorNumber);
                if (actorNumber == 0)
                {
                    continue;
                }
                _playerList.Add(actorNumber);
            }
        }
        else
        {
            List<PhotonPlayer> currentPlayerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
            foreach (PhotonPlayer player in currentPlayerList)
            {
                _playerList.Add(player.ActorNumber);
            }
        }


        _photonView.RPC(nameof(Rpc_SpawnPlayer), RpcTarget.All, _playerList.ToArray());
    }
    
    private void SpawnPlayer()
    {
        for (int i = 0; i < _playerList.Count; i++)
        {
            if (_playerList[i] != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                continue;
            }
            Spawner.GeneratePlayers(i);
        }

        GameManager.Instance.TimeScaleSetting();
    }

    // private void OnDisable()
    // {
    //     EventManager.Instance.OnLoadEnd -= Init;
    // }
    
}
