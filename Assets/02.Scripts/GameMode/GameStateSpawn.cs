using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using UnityEngine;

public class GameStateSpawn : GameModeStateBase
{
    
    public override void Enter()
    {
        // 플레이어 리스트 생성 (현재 방에 있는 플레이어 리스트 생성) > 소한
        if (PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] != null)
        {
            int[] playerList = PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] as int[];
            _gameMode.SpawnPlayer(playerList);
        }
        else
        {
            PhotonPlayer[] players = PhotonNetwork.PlayerList;
            
            int[] playerList = new int[players.Length];
            
            for (int i = 0; i < players.Length; i++)
            {
                playerList[i] = players[i].ActorNumber;
            }
            
            _gameMode.SpawnPlayer(playerList);
        }
        
        _gameMode.CheckState(EModeState.Playing);
    }
    
    public override void Tick()
    {
        
    }
    
    public override void Exit()
    {
            
    }
    
}
