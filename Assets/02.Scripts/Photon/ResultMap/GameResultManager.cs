using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class GameResultManager : Singleton<GameResultManager>
{
    public List<GameResultData> ResultDataList = new List<GameResultData>();
    
    private float _timer = 0;
    private float _EndTime = 10f;
    private bool _nextScene;
    private void Start()
    {
        _nextScene = false;
        List<PhotonPlayer> playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
        List<GameResultData> allResults = new();
        foreach (PhotonPlayer player in playerList)
        {
            int damage = Convert.ToInt32(player.CustomProperties[EProperties.Damage.ToString()]);
            int kill = Convert.ToInt32(player.CustomProperties[EProperties.Kill.ToString()]);
            int survieTime = Convert.ToInt32(player.CustomProperties[EProperties.SurvivorTime.ToString()]);
            int team = Convert.ToInt32(player.CustomProperties[EProperties.Team.ToString()]);
            GameResultData data = new GameResultData(player, damage, kill, survieTime, (EInGameTeam)team);
            Debug.Log(player.ActorNumber +"의 살아남은 시간 : "+ survieTime);

            
            ResultDataList.Add(data);
        }
        
        // 팀별로 묶기
        var groupedTeams = allResults
            .GroupBy(p => p.Team)
            .Select(g => g.OrderByDescending(p => p.SurviveTime).ToList())
            .OrderByDescending(teamGroup => teamGroup[0].SurviveTime) // 팀 대표 생존 시간 기준
            .ToList();
        
        ResultDataList.Clear();
        foreach (var teamGroup in groupedTeams)
        {
            ResultDataList.AddRange(teamGroup); // 생존시간 내림차순된 팀 구성원
        }

        EventManager.Instance.ViewGameResult();
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _EndTime && _nextScene == false)
        {
            _nextScene = true;
            _timer = 0;
       
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(ESceneList.WaitingRoom.ToString());
                
            }
            
        }
    }
    
    
}
