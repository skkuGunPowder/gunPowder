using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using RaycastPro.RaySensors;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class GameResultManager : PhotonSingleton<GameResultManager>
{
    public List<GameResultData> ResultDataList = new List<GameResultData>();
    public PlayerSpawner Spawner;
    private bool _isEnd = false;

    private void Start()
    {
        ReadyReset();
        
        List<PhotonPlayer> playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
 
        foreach (PhotonPlayer player in playerList)
        {
            if(player == null) continue;
            
            int damage = Convert.ToInt32(player.CustomProperties[EProperties.Damage.ToString()]);
            int kill = Convert.ToInt32(player.CustomProperties[EProperties.Kill.ToString()]);
            int survieTime = Convert.ToInt32(player.CustomProperties[EProperties.SurvivorTime.ToString()]);
            int team = Convert.ToInt32(player.CustomProperties[EProperties.Team.ToString()]);
            
            GameResultData data = new GameResultData(player, damage, kill, survieTime, (EInGameTeam)team);

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

            int totalGold = (int)(data.Kill * 50 + data.Damage * 0.1 + data.SurviveTimeRate * 500);
            int totlaEXP = (int)(data.Kill * 100 + data.Damage * 1 + data.SurviveTimeRate * 1000);
           
            data.Gold = totalGold;
            data.EXP = totlaEXP;
            
            if (data.Player.IsLocal)
            {
                CurrencyManager.Instance.AddCurrency(ECurrencyType.Gold, totalGold);
                CurrencyManager.Instance.AddCurrency(ECurrencyType.EXP, totlaEXP);
            }
        }

        Arrange();
        GeneratePlayer();
    }

    private void Arrange()
    {
        // 팀별로 묶기
        // 팀별로 묶고 정렬
        var groupedTeams = ResultDataList
            .GroupBy(p => p.Team)
            .Select(g => g
                .OrderByDescending(p => p.SurviveTime)  // 1. 생존시간 (오래할수록)
                .ThenByDescending(p => p.Kill)          // 2. 킬 (많이할수록)
                .ThenByDescending(p => p.Damage)        // 3. 딜 (많이할수록)
                .ToList())
            .Where(g => g.Count > 0) // 빈 그룹 제거
            .OrderByDescending(teamGroup => teamGroup[0].SurviveTime) // 각 팀 대표의 생존시간 기준
            .ThenByDescending(teamGroup => teamGroup[0].Kill)         // 팀 대표의 킬 기준
            .ThenByDescending(teamGroup => teamGroup[0].Damage)       // 팀 대표의 딜 기준
            .ToList();


        ResultDataList.Clear();

        int currentRank = 1;
        foreach (var teamGroup in groupedTeams)
        {  
            foreach (var data in teamGroup)
            {
                data.Rank = currentRank;
            }

            currentRank ++;
            
            ResultDataList.AddRange(teamGroup); // 팀별 생존시간 내림차순
        }
    }


    private void GeneratePlayer()
    {
        int my = PhotonNetwork.LocalPlayer.ActorNumber;
    
        foreach (GameResultData data in ResultDataList)
        {
            if (my == data.Player.ActorNumber)
            {
                // 같은 등수에서 몇 번째인지 계산
                int count = GetIndexInSameRank(data);
                Spawner.GeneratePlayers(data.Rank, count);
                break;
            }
        }
    }

    
    private int GetIndexInSameRank(GameResultData targetData)
    {
        int index = 0;
        foreach (var data in ResultDataList)
        {
            if (data.Rank == targetData.Rank)
            {
                if (data.Player.ActorNumber == targetData.Player.ActorNumber)
                    return index;
                index++;
            }
        }
        return 0;
    }

    public void LoadScene()
    {
        _isEnd = true;
        
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
            
            PhotonNetwork.LoadLevel(ESceneList.WaitingRoom.ToString());
        }
    }
    
    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        MasterChange();
    }

    private void MasterChange()
    {
        if (_isEnd == false)
        {
            return;
        }
        
        LoadScene();
    }
    private void ReadyReset()
    {
        Hashtable ready = new Hashtable
        {
            { EProperties.IsReady.ToString(), false }
        };
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(ready);
    }
}
