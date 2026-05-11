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
        // 마지막 라운드 우승 팀
        EInGameTeam lastRoundWinner = EInGameTeam.Default;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.LastRoundWinnerTeam.ToString()))
        {
            lastRoundWinner = (EInGameTeam)(int)PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.LastRoundWinnerTeam.ToString()];
        }

        // 팀별로 묶고 팀 내 개인 정렬
        List<List<GameResultData>> groupedTeams = ResultDataList
            .GroupBy(p => p.Team)
            .Select(g => g
                .OrderByDescending(p => p.SurviveTime)
                .ThenByDescending(p => p.Kill)
                .ThenByDescending(p => p.Damage)
                .ToList())
            .Where(g => g.Count > 0)
            .ToList();

        // 팀별 라운드 승리 수와 평균 통계 계산
        groupedTeams.Sort((groupA, groupB) =>
        {
            // 1. 라운드 승리 수 내림차순
            int roundWinsA = GetTeamRoundWins(groupA[0].Player);
            int roundWinsB = GetTeamRoundWins(groupB[0].Player);
            if (roundWinsA != roundWinsB)
            {
                return roundWinsB.CompareTo(roundWinsA);
            }

            // 2. 팀 평균 킬 내림차순
            float avgKillA = (float)SumTeam(groupA, d => d.Kill) / groupA.Count;
            float avgKillB = (float)SumTeam(groupB, d => d.Kill) / groupB.Count;
            if (avgKillA != avgKillB)
            {
                return avgKillB.CompareTo(avgKillA);
            }

            // 3. 팀 평균 딜량 내림차순
            float avgDamageA = (float)SumTeam(groupA, d => d.Damage) / groupA.Count;
            float avgDamageB = (float)SumTeam(groupB, d => d.Damage) / groupB.Count;
            if (avgDamageA != avgDamageB)
            {
                return avgDamageB.CompareTo(avgDamageA);
            }

            // 4. 팀 평균 생존시간 내림차순
            float avgTimeA = (float)SumTeam(groupA, d => d.SurviveTime) / groupA.Count;
            float avgTimeB = (float)SumTeam(groupB, d => d.SurviveTime) / groupB.Count;
            if (avgTimeA != avgTimeB)
            {
                return avgTimeB.CompareTo(avgTimeA);
            }

            // 5. 마지막 라운드 우승 팀 우선
            bool aIsLastWinner = groupA[0].Team == lastRoundWinner;
            bool bIsLastWinner = groupB[0].Team == lastRoundWinner;
            if (aIsLastWinner != bIsLastWinner)
            {
                return aIsLastWinner ? -1 : 1;
            }

            return 0;
        });

        ResultDataList.Clear();

        int currentRank = 1;
        foreach (List<GameResultData> teamGroup in groupedTeams)
        {
            foreach (GameResultData data in teamGroup)
            {
                data.Rank = currentRank;
            }
            currentRank++;
            ResultDataList.AddRange(teamGroup);
        }
    }

    private int GetTeamRoundWins(PhotonPlayer player)
    {
        if (player.CustomProperties.ContainsKey(EProperties.RoundWins.ToString()))
        {
            return (int)player.CustomProperties[EProperties.RoundWins.ToString()];
        }
        return 0;
    }

    private int SumTeam(List<GameResultData> group, Func<GameResultData, int> selector)
    {
        int sum = 0;
        foreach (GameResultData data in group)
        {
            sum += selector(data);
        }
        return sum;
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
