using System;
using System.Collections.Generic;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using UnityEngine;

public class DamageChecker : Singleton<DamageChecker>
{
    private PhotonView _photonView;

    private int _currentTopPlayer;                           // 처음 1등은 방장
    
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
        // 플레이어 모아서
        // 뷰 찾아서 같은 애면 스탯 바꿔줌
        // 특정 ActorNumber의 플레이어 찾기
        if (PhotonNetwork.CurrentRoom.Players.TryGetValue(playerNumber, out PhotonPlayer player))
        {
            // 모든 PlayerStat 컴포넌트를 찾아서 해당 플레이어의 스탯 업데이트
            PlayerStat[] allPlayerStats = FindObjectsByType<PlayerStat>(FindObjectsSortMode.None);
            
            foreach (PlayerStat playerStat in allPlayerStats)
            {
                PhotonView playerPhotonView = playerStat.GetComponent<PhotonView>();
                if (playerPhotonView != null && playerPhotonView.OwnerActorNr == playerNumber)
                {
                    // 해당 플레이어의 스탯 업데이트
                    playerStat.SetPlayerGunPowderCountAndLife(gunpowder, life);
                    break;
                }
            }
        }

        EventManager.Instance.PlayerDataChange(gunpowder, life, playerNumber);
    }
    
    private void CalculateScore(int gunpowder, int life, int playerNumber)
    {
        if (life == 0)
        {
            return;
        }
        
        int score = (life * 300) + gunpowder;
        
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
