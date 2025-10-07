using System;
using System.Collections.Generic;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using UnityEngine;

public class DamageChecker : Singleton<DamageChecker>
{
    private PhotonView _photonView;

    private int _currentTopPlayer;                           
    
    private Dictionary<int, int> _playerScoreDictionary;
    private List<int>  _playerList;
    public  List<int> PlayerList => _playerList;
    
    private List<PhotonView> _playerPhotonViewList;
    
    
    protected override void Awake()
    {
        base.Awake();
        _photonView = GetComponent<PhotonView>();
        _playerPhotonViewList = new List<PhotonView>();
    }

    private void Start()
    {
        Init();
        EventManager.Instance.OnPlayerChanged += LeftPlayer;
    }
    private void Init()
    {
        _playerList = new List<int>();
        _playerScoreDictionary = new Dictionary<int, int>();

        List<PhotonPlayer> playerlist = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
        
        if (playerlist.Count == 0)
        {
            return;       
        }

        foreach (var player in playerlist)
        {
            _playerList.Add(player.ActorNumber);
            _playerScoreDictionary.Add(player.ActorNumber, RoomStatManager.Instance.PlayerLife * RoomStatManager.Instance.PlayerGunpowder);
        }

        _currentTopPlayer = -1;
        _playerScoreDictionary.Add(_currentTopPlayer, -1); // 초기화
    }

    private void LeftPlayer(PhotonPlayer leftPlayer)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        int playerNumber = leftPlayer.ActorNumber;
        
        _playerScoreDictionary[leftPlayer.ActorNumber] = -1;
        
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

        if (topActor != _currentTopPlayer)
        {
            _currentTopPlayer = topActor;
            _photonView.RPC(nameof(RPC_RequestTopPlayer), RpcTarget.All, topActor);
        }
        
    }
    public void SetPlayerView()
    {
        PlayerStat[] allPlayerStats = FindObjectsByType<PlayerStat>(FindObjectsSortMode.None);
        foreach (PlayerStat playerStat in allPlayerStats)
        {
            PhotonView playerPhotonView = playerStat.GetComponent<PhotonView>();
            if (playerPhotonView != null)
            {
                _playerPhotonViewList.Add(playerPhotonView);
            }
        }
    }

    public void ActiveKillLog(int killer, bool isNormal, int death)
    {
        if (GameManager.Instance.CurrentGameState != EGameState.Playing)
        {
            return;
        }
        
        EventManager.Instance.OnUpdateLog(killer, isNormal, death);
    }
    
    public void RPC_RequestDamage(int gunpowder, int life, int attacker, int player)
    {
        
        if (GameManager.Instance.CurrentGameState != EGameState.Playing)
        {
            return;
        }
        
        PlayerDataChange(gunpowder, life, player, attacker);
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        CalculateScore(gunpowder, life, player);
    }
    
    private void PlayerDataChange(int gunpowder, int life, int playerNumber, int attacker)
    {
        foreach (PhotonView view in _playerPhotonViewList)
        {
            if (view != null && view.OwnerActorNr == playerNumber)
            {
                PlayerStat playerStat = view.GetComponent<PlayerStat>();
                // 해당 플레이어의 스탯 업데이트
                playerStat.SetPlayerGunPowderCountAndLife(gunpowder, life);
                break;
            }
        }
        
        EventManager.Instance.PlayerDataChange(gunpowder, life, playerNumber,attacker);
    }
    
    private void CalculateScore(int gunpowder, int life, int playerNumber)
    {
        if (life == 0)
        {
            _playerScoreDictionary[playerNumber] = 0;
            return;
        }
        
        int score = life * gunpowder;
        
        _playerScoreDictionary[playerNumber] = score;
        CheckTopPlayer(playerNumber);
    }

    private void CheckTopPlayer(int playerNumber)
    {
        if (playerNumber == _currentTopPlayer)
        {
            return;       
        }
        
        int topActor = _currentTopPlayer;
        int topScore = _playerScoreDictionary[topActor];

        if (_playerScoreDictionary[playerNumber] > topScore)
        {
            _currentTopPlayer = playerNumber;
            _photonView.RPC(nameof(RPC_RequestTopPlayer), RpcTarget.All, _currentTopPlayer);
        }
    }
    
    [PunRPC]
    private void RPC_RequestTopPlayer(int topActor)
    {
        EventManager.Instance.SetTopPlayer(topActor);
    }
    
    private void OnDisable()
    {
        EventManager.Instance.OnPlayerChanged -= LeftPlayer;
    }
}
