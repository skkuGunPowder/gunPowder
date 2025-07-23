using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
[RequireComponent(typeof(PhotonView))]
public class PlayerSettingManager : Singleton<PlayerSettingManager>
{
    private List<int> _playerList = new List<int>(); //현재 있는 플레이어들
    public List<int> PlayerList => _playerList;
    public int PlayerLife;
    public int PlayerGunpowder;
    public int PlayerDeclinePowder;         // 몇초당 1 감소 의 몇 초
    public int PlayTime;
    
    private Room _room;
    private PhotonView _photonView;
    
    [Header("더미 데이터")]
    public int Gunpowder;
    public int Life;
    
    public PlayerSpawner Spawner;

    public event Action<int,int,int> OnDataChanged;         // 언제? :
    
    // 현재 룸 프로퍼티 가져오기
    protected override void Awake()
    {
        base.Awake();
        
        PlayerLife = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Life}"].ToString());
        PlayerGunpowder = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Gunpowder}"].ToString()); 
        PlayerDeclinePowder = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.DeclinePowder}"].ToString());
        PlayTime = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.PlayTime}"].ToString());
        
        _photonView = GetComponent<PhotonView>();
        _room = PhotonNetwork.CurrentRoom;
    }

    // 플레이어 스탯에 연결해줄 것들 세팅하기
    private void Start()
    {
        // 캐릭터 순번 세팅
        SpawnSetting();
        
        // 플레이어 생성
        SpawnPlayer();
        
        Hashtable load = new Hashtable()
        {
            { EProperties.IsLoad.ToString(), true },
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(load); 
        // 플레이어 스탯 추가해주기
    }
    
    [PunRPC]
    private void Rpc_SpawnPlayer(int[] playerList)
    {
        _playerList = new List<int>(playerList);
    }
    
    // 프로퍼티 불러오기  => 플레이어 리스트
    private void SpawnSetting()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] is int[] actorNumbers)
        {
            List<int> currentPlayerList = new List<int>(actorNumbers);
            
            foreach (int actorNumber in currentPlayerList)
            {
                if (actorNumber == 0)
                {
                    continue;
                }
                
                _playerList.Add(actorNumber);
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
        
        
    }
    
    [PunRPC]
    public void OnClickReduce(int gunpowder, int life, int value, PhotonMessageInfo info)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        Gunpowder -= value;
        
        if (Gunpowder <= 0)
        {
            Life -= 1;
            Gunpowder = 100;
        }
        
        _photonView.RPC(nameof(RPC_RequestDamage),RpcTarget.All, gunpowder, life, info.Sender.ActorNumber);
    }
    public void RequestTakeDamage(int gunpowder, int life, int value)
    {
        if (_photonView.IsMine == false)
        {
            return;
        }
        _photonView.RPC(nameof(OnClickReduce),RpcTarget.MasterClient, gunpowder, life, value);
    }
    
    [PunRPC]
    public void RPC_RequestDamage(int gunpowder, int life, int playerNumber)
    {
        OnDataChanged?.Invoke(playerNumber, gunpowder, life);
    }
    // [PunRPC]
    // public void RPC_TakeDamage(int gunpowder, int life, int playerNumber)
    // {
    //     if (_photonView.IsMine == false)
    //     {
    //         return;    
    //     }
    //     
    //     _photonView.RPC(nameof(RPC_RequestDamage), RpcTarget.All, gunpowder, life, playerNumber);
    // }
    
}
