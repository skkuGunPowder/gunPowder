// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Photon.Pun;
// using UnityEngine;
// using PhotonPlayer = Photon.Realtime.Player;
// public class Test_PlayerInfo : MonoBehaviour
// {
//     public List<GameResultData>ResultDataList = new List<GameResultData>();
//     private void Start()
//     {
//         List<int> playerList = new List<int>() { 1, 2, 3, 4 };
//             
//         Debug.Log($"결과 : 플레이어 리스트 {playerList.Count}");
//         List<GameResultData> allResults = new();
//
//         for (int i = 0; i < playerList.Count; i++)
//         {
//             int damage = 100 * i;
//             int kill = 1 + i;
//             int survieTime = 100 * i;
//             int team = 1;
//
//             if (i == 3)
//             {
//                 team = 2;
//             }
//             GameResultData data = new GameResultData(i, damage, kill, survieTime, (EInGameTeam)team);
//             ResultDataList.Add(data);   
//             
//         }
//         
//         // 팀별로 묶기
//         var groupedTeams = ResultDataList
//             .GroupBy(p => p.Team)
//             .Select(g => g.OrderByDescending(p => p.SurviveTime).ToList())
//             .Where(g => g.Count > 0) // 빈 그룹 제거
//             .OrderByDescending(teamGroup => teamGroup[0].SurviveTime) // 각 팀 대표의 생존시간 기준
//             .ToList();
//         
//         Debug.Log($"그룹화 결과 : 그룹화 1 {groupedTeams.Count}");
//         Debug.Log($"결과 : 정렬 전 데이터 리스트 {ResultDataList.Count}");
//         
//         
//         ResultDataList.Clear();
//         foreach (var teamGroup in groupedTeams)
//         {
//             Debug.Log(teamGroup.Count);
//             foreach (var team in teamGroup)
//             {
//                 Debug.Log($"{team.Team} : {team.SurviveTime}");
//             }
//             ResultDataList.AddRange(teamGroup); // 팀별 생존시간 내림차순
//         }
//         Debug.Log($"결과 : 데이터 리스트 {ResultDataList.Count}");
//     }
//
// }
