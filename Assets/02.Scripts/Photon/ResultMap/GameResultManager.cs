using System;
using System.Collections.Generic;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;

public class GameResultManager : Singleton<GameResultManager>
{
    public List<GameResultData> ResultDataList = new List<GameResultData>();
    
    private void Start()
    {
        List<PhotonPlayer> playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
        foreach (PhotonPlayer player in playerList)
        {
            int damage = Convert.ToInt32(player.CustomProperties[EProperties.Damage.ToString()]);
            int kill = Convert.ToInt32(player.CustomProperties[EProperties.Kill.ToString()]);
            int survieTime = Convert.ToInt32(player.CustomProperties[EProperties.SurvivorTime.ToString()]);
            
            GameResultData data = new GameResultData(player, damage, kill, survieTime);
            
            ResultDataList.Add(data);
        }
        ResultDataList.Sort((a, b) =>
        {
            int surviveCompare = b.SurviveTime.CompareTo(a.SurviveTime);
            if (surviveCompare != 0) return surviveCompare;

            int killCompare = b.Kill.CompareTo(a.Kill);
            if (killCompare != 0) return killCompare;

            return b.Damage.CompareTo(a.Damage);
        });
        EventManager.Instance.ViewGameResult();
    }
}
