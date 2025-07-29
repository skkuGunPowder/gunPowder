using System;
using System.Collections.Generic;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using UnityEngine;

public class DamageChecker : Singleton<DamageChecker>
{
    private PhotonView _photonView;

    private int _currentTopPlayer;                 // 처음 1등은 방장
    
    private Dictionary<int, int> _playerScoreDictionary;
    private List<int>  _playerList;
    public  List<int> PlayerList => _playerList;
    public LoadSceneChecker LoadSceneChecker;
    
    
    protected override void Awake()
    {
        base.Awake();
        _photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        _playerList = new List<int>();
        _playerScoreDictionary = new Dictionary<int, int>();
        
        List<PhotonPlayer> playerlist = new List<PhotonPlayer>(PhotonNetwork.PlayerList);

        foreach (var player in playerlist)
        {
            _playerList.Add(player.ActorNumber);
            _playerScoreDictionary.Add(player.ActorNumber, RoomStatManager.Instance.PlayerLife * RoomStatManager.Instance.PlayerGunpowder);
        }
        
        _currentTopPlayer = PlayerList[0];
    }
    
    public void RPC_RequestDamage(int gunpowder, int life, int player)
    {
        PlayerDataChange(gunpowder, life, player);
        
        if (GameManager.Instance.CurrentGameState == EGameState.Waiting)
        {
            return;
        }
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        CalculateScore(gunpowder, life, player);
    }
    
    private void PlayerDataChange(int gunpowder, int life, int playerNumber)
    {
        EventManager.Instance.PlayerDataChange(gunpowder, life, playerNumber);
    }
    
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
        int topActor = _currentTopPlayer;
        int topScore = _playerScoreDictionary[topActor];

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
        EventManager.Instance.SetTopPlayer(topActor);
    }
}
