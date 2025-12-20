using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class BattleMode : GameModeBase
{
    /// <summary>
    /// 1. 플레이어 리스트 받아와서 소환하기
    /// </summary>
    
    protected override void Start()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] != null)
        {
            int[] playerList = PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] as int[];
            SpawnPlayer(playerList);
        }
        else
        {
            PhotonPlayer[] players = PhotonNetwork.PlayerList;
            
            int[] playerList = new int[players.Length];
            
            for (int i = 0; i < players.Length; i++)
            {
                playerList[i] = players[i].ActorNumber;
            }
            
            SpawnPlayer(playerList);
            
        }
    }
    
    private void SpawnPlayer(int[] playerList)
    {
        for (int i = 0; i < playerList.Length; i++)
        {
            if (playerList[i] != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                continue;
            }
            
            _playerSpawner.GeneratePlayers(i);
        }
    }
    
}
