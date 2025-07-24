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
    public int PlayerLife;
    public int PlayerGunpowder;
    public int PlayerDeclinePowder;         // 몇초당 1 감소 의 몇 초
    public int PlayTime;
    
    private Room _room;
    private PhotonView _photonView;
    
    public PlayerSpawner Spawner;
    public LoadSceneChecker LoadSceneChecker;
    public event Action<int,int,int> OnDataChanged;         // 언제? :
    public event Action OnInitCharacter;
    // 현재 룸 프로퍼티 가져오기
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("awake");
        _photonView = GetComponent<PhotonView>();
        _room = PhotonNetwork.CurrentRoom;
        //
        // LoadSceneChecker.OnLoadFinished += Init;

        Debug.Log($"{int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Life}"].ToString())}");
        Debug.Log($"{PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Life}"]}");
        
    }

    private void Start()
    {
        Debug.Log("start");
        PlayerLife = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Life}"].ToString());
        PlayerGunpowder = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Gunpowder}"].ToString()); 
        PlayerDeclinePowder = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.DeclinePowder}"].ToString());
        PlayTime = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.PlayTime}"].ToString());
        
        Debug.Log($"{int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Life}"].ToString())}");
        Debug.Log($"{PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Life}"]}");
        
        StartCoroutine(spawnPlayer());
        
    }

    private IEnumerator spawnPlayer()
    {
        yield return new WaitForSeconds(1f);
        
        PlayerLife = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Life}"].ToString());
        PlayerGunpowder = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Gunpowder}"].ToString()); 
        PlayerDeclinePowder = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.DeclinePowder}"].ToString());
        PlayTime = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.PlayTime}"].ToString());
        
        Init();
    }
    public void Init()
    {
        

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
        Debug.Log("spawn");
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
            Spawner.GeneratePlayers(i, PlayerGunpowder, PlayerLife);
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
    }
    
    [PunRPC]
    public void RPC_RequestDamage(int gunpowder, int life, int playerNumber)
    {
        OnDataChanged?.Invoke(playerNumber, gunpowder, life);
    }

}
