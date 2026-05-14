using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using UnityEngine;

public class GameStateSpawn : GameModeStateBase
{

    public override void Enter()
    {
        int[] playerList;
        
        if (PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] != null)
        {
            playerList = PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] as int[];
        }
        else
        {
            PhotonPlayer[] players = PhotonNetwork.PlayerList;
            playerList = new int[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                playerList[i] = players[i].ActorNumber;
            }
        }

        // 모든 클라이언트가 동일한 결과를 내도록 ActorNumber 합산을 seed로 사용
        int seed = 0;
        foreach (PhotonPlayer p in PhotonNetwork.PlayerList) seed += p.ActorNumber;
        System.Random rng = new System.Random(seed);
        
        for (int i = playerList.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (playerList[i], playerList[j]) = (playerList[j], playerList[i]);
        }

        for (int i = 0; i < playerList.Length; i++)
        {
            if (playerList[i] != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                continue;
            }
            
            _gameMode.MyPlayer = _gameMode.PlayerSpawner.GeneratePlayers(i);
            break;
        }

        OnSpawnComplete();
    }

    // 스폰 완료 후 첫 라운드면 BombSelect, 이후는 Playing으로 전환
    private void OnSpawnComplete()
    {
        EModeState nextState = _gameMode.IsFirstSpawn ? EModeState.BombSelect : EModeState.Playing;
        _gameMode.CheckChangeComplete(nextState);
    }
    public override void Tick()
    {
        
    }
    
    public override void Exit()
    {
            
    }
    
}
