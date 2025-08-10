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
    
    private void Start()
    {
        List<PhotonPlayer> playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
 
        Debug.Log($"결과 : 플레이어 리스트 {playerList.Count}");
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
        
        int maxDamage = Mathf.Max(ResultDataList.Max(d => d.Damage),1);
        int maxKill = Mathf.Max(ResultDataList.Max(k => k.Kill),1);
        int maxSurviveTime = Mathf.Max(ResultDataList.Max(s =>s.SurviveTime),1);

        foreach (var data in ResultDataList)
        {   
            data.CalculateDamageRate(maxDamage);
            data.CalculateKillRate(maxKill);
            data.CalculateSurviveTimeRate(maxSurviveTime);
        }
        
        Arrange();
    }

    private void Arrange()
    {
        // 팀별로 묶기
        var groupedTeams = ResultDataList
            .GroupBy(p => p.Team)
            .Select(g => g.OrderByDescending(p => p.SurviveTime).ToList())
            .Where(g => g.Count > 0) // 빈 그룹 제거
            .OrderByDescending(teamGroup => teamGroup[0].SurviveTime) // 각 팀 대표의 생존시간 기준
            .ToList();
        Debug.Log($"그룹화 결과 : 그룹화 1 {groupedTeams.Count}");
        Debug.Log($"결과 : 정렬 전 데이터 리스트 {ResultDataList.Count}");
        
        ResultDataList.Clear();

        int currentRank = 1;
        foreach (var teamGroup in groupedTeams)
        {  
            foreach (var data in teamGroup)
            {
                data.Rank = currentRank;
            }

            // 등수 건너뛰기: 해당 팀 인원 수 만큼 증가
            currentRank += teamGroup.Count;
            
            ResultDataList.AddRange(teamGroup); // 팀별 생존시간 내림차순
            
            foreach (var team in teamGroup)
            {
                Debug.Log(team.Team.ToString());
            }
        }
        Debug.Log($"결과 : 데이터 리스트 {ResultDataList.Count}");
    }
    
    private void LoadScene()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(ESceneList.WaitingRoom.ToString());
                
        }
    }
}
