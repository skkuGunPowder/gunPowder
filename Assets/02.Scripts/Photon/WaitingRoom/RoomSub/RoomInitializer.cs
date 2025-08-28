using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonPlayer = Photon.Realtime.Player;

public class RoomInitializer
{
    public void Init(RoomManager roomManager)
    {
        SetProperties(roomManager);
        SetCurrentMap(roomManager);
        GeneratePlayer(roomManager);
    }
    
    private void SetProperties(RoomManager roomManager)
    {
        Hashtable ready = new Hashtable
        {
            { EProperties.IsReady.ToString(), false },
            { EProperties.IsDead.ToString(), false },
            { EProperties.IsLoad.ToString(), false}
        };
        
        if (PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()] == null)
        {
            PhotonPlayer[] players = PhotonNetwork.PlayerList;
            HashSet<EInGameTeam> usedTeams = new HashSet<EInGameTeam>();
            
            foreach (PhotonPlayer player in players)
            {
                if (player.CustomProperties.TryGetValue(EProperties.Team.ToString(), out object teamObj))
                {
                    EInGameTeam team = (EInGameTeam)teamObj;
                    usedTeams.Add(team);
                }
            }

            // 가능한 팀 중에서 사용되지 않은 팀 찾기
            EInGameTeam myTeam = EInGameTeam.Red; // 기본값
            foreach (EInGameTeam team in Enum.GetValues(typeof(EInGameTeam)))
            {
                if (!usedTeams.Contains(team))
                {
                    myTeam = team;
                    break;
                }
            }
            
            ready.Add(EProperties.Team.ToString(), (int)myTeam);
        }
        else
        {
            roomManager.SelectedTeam = (EInGameTeam)PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()];
        }
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(ready);
    }
    
    // 현재 방의 맵이 무엇인가?
    private void SetCurrentMap(RoomManager roomManager)
    {
        Room room = PhotonNetwork.CurrentRoom;
     
        roomManager.SelectedMap = (EMap)room.CustomProperties[ERoomProperties.MapSelected.ToString()];
        EventManager.Instance.MapChanged();
    }

    private void GeneratePlayer(RoomManager roomManager)
    {
        roomManager.Spawner.GeneratePlayers(0);
    }
    
}
