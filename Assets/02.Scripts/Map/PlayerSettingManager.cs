using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonPlayer = Photon.Realtime.Player;

[RequireComponent(typeof(PhotonView))]
public class PlayerSettingManager : Singleton<PlayerSettingManager>
{
    private List<int> _playerList = new List<int>(); //현재 있는 플레이어들
    public List<int> PlayerList => _playerList;
    
    private Room _room;
    private PhotonView _photonView;
    
    public int PlayTime;

    private Dictionary<int, int> _playerScoreDictionary = new Dictionary<int, int>();
    
    public PlayerSpawner Spawner;
    public LoadSceneChecker LoadSceneChecker;
    
    public event Action<int> OnTopPlayerChanged; 
    public event Action<int,int,int> OnDataChanged;         // 언제? :
    public event Action OnInitCharacter;
    
    // 현재 룸 프로퍼티 가져오기
    protected override void Awake()
    {
        base.Awake();
        _photonView = GetComponent<PhotonView>();
        _room = PhotonNetwork.CurrentRoom;
        
        LoadSceneChecker.OnLoadFinished += Init;
        
    }

    public void Init()
    {
        PlayTime = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.PlayTime}"].ToString());
        
        Hashtable dead = new Hashtable()
        {
            { EProperties.IsDead.ToString(), false }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(dead);
        // 캐릭터 순번 세팅
        SpawnSetting();

        // 플레이어 스탯 추가해주기   
    }
    
    [PunRPC]
    private void Rpc_SpawnPlayer(int[] playerList)
    {
        _playerList = new List<int>(playerList);
        SpawnPlayer();
    }
    
    // 프로퍼티 불러오기  => 플레이어 리스트
    private void SpawnSetting()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        if (PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] != null)
        {
            int[] players = PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] as int[];   
            List<int> currentPlayerList = new List<int>(players);
            
            foreach (int actorNumber in currentPlayerList)
            {
                if (actorNumber == 0)
                {
                    continue;
                }

                int score = RoomStatManager.Instance.PlayerLife * RoomStatManager.Instance.PlayerGunpowder;
                _playerScoreDictionary.Add(actorNumber, score);
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
            Debug.Log(_playerList[i]);
            Debug.Log( PhotonNetwork.LocalPlayer.ActorNumber);
            if (_playerList[i] != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                continue;
            }
        
            Debug.Log($"{PhotonNetwork.LocalPlayer.ActorNumber} 가 소환한당");    
            Spawner.GeneratePlayers(i);
        }
        
        OnInitCharacter?.Invoke();
    }

    public void RequestTakeDamage(int gunpowder, int life, int value)
    {
        if (_photonView.IsMine == false)
        {
            return;
        }
        
        _photonView.RPC(nameof(RPC_RequestDamage),RpcTarget.All, gunpowder, life, value);
        _photonView.RPC(nameof(CalculateScore),RpcTarget.MasterClient,gunpowder, life);
    }
    
    [PunRPC]
    public void RPC_RequestDamage(int gunpowder, int life, int playerNumber)
    {
        OnDataChanged?.Invoke(playerNumber, gunpowder, life);
    }

    [PunRPC]
    private void CalculateScore(int gunpowder, int life, int playerNumber)
    {
        if (life == 0)
        {
            return;
        }
        
        int score = playerNumber * gunpowder;
        _playerScoreDictionary[playerNumber] = score;
        CheckTopPlayer();
    }
    
    
    private void CheckTopPlayer()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        int topActor = -1;
        int topScore = int.MinValue;

        foreach (var kvp in _playerScoreDictionary)
        {
            if (kvp.Value > topScore)
            {
                topScore = kvp.Value;
                topActor = kvp.Key;
            }
        }
        _photonView.RPC(nameof(RPC_RequestTopPlayer), RpcTarget.All, topActor);
    }

    [PunRPC]
    private void RPC_RequestTopPlayer(int topActor)
    {
        OnTopPlayerChanged?.Invoke(topActor);
    }
}
