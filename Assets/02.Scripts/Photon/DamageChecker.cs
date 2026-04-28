using System;
using System.Collections.Generic;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using UnityEngine;

public class DamageChecker : Singleton<DamageChecker>
{
    /// <summary>
    /// 플레이어 스탯 변경, UI변경
    /// 
    /// </summary>
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
        // 중복 구독 방어
        EventManager.Instance.OnPlayerChanged -= LeftPlayer;
        EventManager.Instance.OnPlayerChanged += LeftPlayer;
        EventManager.Instance.OnGameStart -= SetPlayerView;
        EventManager.Instance.OnGameStart += SetPlayerView;
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
            _playerScoreDictionary.Add(player.ActorNumber, RoomStatManager.Instance.PlayerLife * 150);
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

        if (leftPlayer == null || _playerScoreDictionary == null)
        {
            return;
        }

        _playerScoreDictionary[leftPlayer.ActorNumber] = -1;

        int topActor = _currentTopPlayer;
        if (!_playerScoreDictionary.ContainsKey(topActor))
        {
            return;
        }

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
    
    public void RPC_RequestDamage(int hp, int life, int attacker, int player)
    {

        if (GameManager.Instance.CurrentGameState != EGameState.Playing)
        {
            return;
        }

        PlayerDataChange(hp, life, player, attacker);

        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        CalculateScore(hp, life, player);
    }

    private void PlayerDataChange(int hp, int life, int playerNumber, int attacker)
    {
        foreach (PhotonView view in _playerPhotonViewList)
        {
            if (view != null && view.OwnerActorNr == playerNumber)
            {
                PlayerStat playerStat = view.GetComponent<PlayerStat>();
                playerStat.SetPlayerHPAndLife(hp, life);
                break;
            }
        }

        EventManager.Instance.PlayerDataChange(hp, life, playerNumber, attacker);
    }

    private void CalculateScore(int hp, int life, int playerNumber)
    {
        if (life == 0)
        {
            _playerScoreDictionary[playerNumber] = 0;
            return;
        }

        int score = (life * 150) + hp; // HP 고정 150 * 생명 + 현재 HP

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
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnPlayerChanged -= LeftPlayer;
            EventManager.Instance.OnGameStart -= SetPlayerView;
        }
    }
}
