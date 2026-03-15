using System.Collections.Generic;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using UnityEngine;

public class TeamScore
{
    public EInGameTeam team;
    public int score;
    
    public TeamScore(EInGameTeam team, int score)
    {
        this.team = team;
        this.score = 0;
    }
    
    public void AddScore()
    {
        
        this.score += 1;
    }
}

public class GameStateRound : GameModeStateBase
{
    private Dictionary<EInGameTeam, TeamScore> _roundTeamCount = new Dictionary<EInGameTeam, TeamScore>();
    private const int ROUND_SCORE_LIMIT = 5;
    // 현재 라운드 체크, 
    
    
    
    // 초기화 (팀 개수 체크, 승리 라운드 0으로 설정)
    public override void Initialize(GameModeBase gameMode)
    {
        base.Initialize(gameMode);

        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        
        foreach (PhotonPlayer player in players)
        {
            EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
            _roundTeamCount.TryAdd(team, new TeamScore(team, 0));
        }
        
    }
    // 라운드 종료 
    
    public override void Enter()
    {
        ProduceScore();
    }
    
    // 우승팀 점수 올리기 >> 라운드 종료 체크
    private void AddScore(EInGameTeam team)
    {
        _roundTeamCount[team].AddScore();
        EndCheck(team);
    }
    
    private void EndCheck(EInGameTeam team)
    {
        int score = _roundTeamCount[team].score;

        if (score >= ROUND_SCORE_LIMIT)
        {
            // 게임 종료
            _gameMode.GameOver();   
        }
    }

    // 점수 연출
    private void ProduceScore()
    {
        // 연출
        
        // 점수 추가
        AddScore(_gameMode.WinningTeam);
    }
    public override void Tick()
    {
        
    }

    public override void Exit()
    {
        
    }
    
}
