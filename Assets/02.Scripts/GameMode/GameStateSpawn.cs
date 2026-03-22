using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using UnityEngine;

public class GameStateSpawn : GameModeStateBase
{
    
    public override void Enter()
    {
        Debug.LogWarning("Enter State : GameStateSpawn");
        
        // 플레이어 리스트 생성 (현재 방에 있는 플레이어 리스트 생성) > 소한
        if (PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] != null)
        {
            int[] playerList = PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] as int[];
            _gameMode.SpawnPlayer(playerList,OnSpawnComplete);
        }
        else
        {
            PhotonPlayer[] players = PhotonNetwork.PlayerList;
            
            
            int[] playerList = new int[players.Length];
            
            for (int i = 0; i < players.Length; i++)
            {
                playerList[i] = players[i].ActorNumber;
            }
            
            _gameMode.SpawnPlayer(playerList, OnSpawnComplete);
        }
        
    }
    
    // [Fix #2] 스폰이 완료된 후 Playing 상태로 전환
    private void OnSpawnComplete()
    {
        // 스폰의 경우 룸 프로퍼티를 보내지 않고 바로 방장에게 변경 요청 보내도록
        // 모두의 스폰 시간이 다르기 때문
        _gameMode.CheckChangeComplete(EModeState.Playing); 
    }
    public override void Tick()
    {
        
    }
    
    public override void Exit()
    {
            
    }
    
}
