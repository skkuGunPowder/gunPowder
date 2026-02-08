using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class TeamTracker : MonoBehaviour
{
    // 현재 팀 상태를 알려주는 클래스
    // 현재 존재하는 팀 팀원 수 체크
    
    private Dictionary<EInGameTeam, int> _teamCount = new Dictionary<EInGameTeam, int>(); // 살아 있는 팀원 수 : 팀 / 팀원 수

    public void Init()
    {
        TeamSetting();
        // 시작 전 플레이어 수가 충분한가?
        // CheckGameEnd();
    }

    /// <summary>
    /// 팀마다 플레이어가 몇명 존재하는지 설정
    /// </summary>
    private void TeamSetting()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;

        foreach (PhotonPlayer player in players)
        {
            EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];

            if (_teamCount.ContainsKey(team))
            {
                _teamCount[team]++;
            }
            else
            {
                _teamCount.Add(team, 1);
            }
        }
    }
    public void SubPlayer(EInGameTeam team)
    {
        //팀에서 팀원 감소시키기
        _teamCount[team]--;
    }
    
    public void AddPlayer()
    {
        
    }
    
    // 팀, 플레이어 수에 따라 종료되는 조건
    public int LastTeamCheck()
    {
        int count = 0;
     
        // 살아있는 팀원이 있는가?
        foreach (int value in _teamCount.Values)
        {
            if(value > 0) 
            {
                count++; // ( value > 0)
            }
        }
        
        return count;
    }

    
    // return 내 팀에 남아있는 팀원이 있는가?
    public bool LastAttackCheck(EInGameTeam team)
    {
        return _teamCount[team] <= 0;
    }
}