using System.Collections.Generic;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using UnityEngine;

public class TeamScore
{
    public EInGameTeam team;
    public int score;
    
    public TeamScore(EInGameTeam team)
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
    private const int ROUND_SCORE_LIMIT = 5; // 라운드 설정
    
    // 우승자 확인용 변수
    private int _maxTime = int.MinValue;
    private float _damage = float.MinValue;
    private int _maxHp = int.MinValue;
    private int _playerCount = 0;
    
    // 자신의 결과 저장
    private float _myDamage = 0;
    private float _myKill = 0;
    private int _mySurvivorTime = 0;
    
    public EInGameTeam _winningTeam;
    
    
    // 초기화 (팀 개수 체크, 승리 라운드 0으로 설정)
    public override void Initialize(GameModeBase gameMode)
    {
        base.Initialize(gameMode);

        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        
        _roundTeamCount.Clear();
        
        foreach (PhotonPlayer player in players)
        {
            EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
            _roundTeamCount.TryAdd(team, new TeamScore(team));
        }
        
    }
    // 라운드 종료 
    
    public override void Enter()
    {
        Init();
        EventManager.Instance.OnRoundEnd += EndCheck;
        Debug.Log("round enter");
        RequestMyPlayerHealth();
    }
    
    // 초기화
    private void Init()
    {
        _maxTime = int.MinValue;
        _damage = float.MinValue;
        _maxHp = int.MinValue;
        _playerCount = 0;
    }
    // 우승팀 체크 >> 방장이 체크 후 플레이어들에게 전달
    private void CheckWinningTeam(int hp, PhotonPlayer player)
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        
        _playerCount += 1;
        
        if (players.Length == 1)
        {
            _gameMode.GameOver();   // 플레이어가 한명이라면 바로 종료
            return;
        }
        
        int time = (int)player.CustomProperties[EProperties.SurvivorTime.ToString()];
        
        float damage = (float)player.CustomProperties[EProperties.Damage.ToString()];
        
        // 현재 플레이어가 더 우세한지 체크
        bool isBetterHP = hp > _maxHp;
        bool isBetterTime = _maxHp == hp && time > _maxTime;
        bool isSameHealthButMoreDamage = _maxHp == hp && time == _maxTime && damage > _damage;

        if (isBetterHP || isBetterTime || isSameHealthButMoreDamage)
        {
            _maxHp = hp;
            _maxTime = time;
            _damage = damage;
            _winningTeam = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
        }
        

        if (players.Length == _playerCount)
        {
            SetWinningTeam(_winningTeam);
        }
    }
    
    private void RequestMyPlayerHealth()
    {
        // 방장에게 자신의 체력 전달
        PlayerStat stat = _gameMode.MyPlayer.GetComponent<PlayerStat>();

        int hp = stat.CurrentPlayerGunPowderCount;
        _photonView.RPC(nameof(RPC_RequestMyPlayerHealth), RpcTarget.MasterClient, hp);
    }
    
    [PunRPC]
    private void RPC_RequestMyPlayerHealth(int hp,PhotonMessageInfo info)
    {
        PhotonPlayer sender = info.Sender;
        
        CheckWinningTeam(hp, sender);
    }
    
    // 우승팀 점수 올리기 >> 라운드 종료 체크
    private void AddScore(EInGameTeam team)
    {
        _roundTeamCount[team].AddScore();
        EventManager.Instance.ScoreUpdate(team, _roundTeamCount[team].score);
    }
    
    private void EndCheck()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        foreach (TeamScore teamScore in _roundTeamCount.Values)
        {
            if (teamScore.score >= ROUND_SCORE_LIMIT)
            {
                _gameMode.RequestStateChange(EModeState.Over);
                EventManager.Instance.OnRoundEnd -= EndCheck;
                return;
            }
        }

        _gameMode.RequestStateChange(EModeState.Cartirdge);
    }

    // 점수 연출
    private void ProduceScore()
    {
        AddScore(_winningTeam);
        
        // 연출
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        PhotonNetwork.DestroyAll();
         
    }
    
    public void SetWinningTeam(EInGameTeam team)
    {
        _winningTeam = team;
        ProduceScore();
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        _photonView.RPC(nameof(RPC_WinningTeam), RpcTarget.Others, team);
    }
    
    [PunRPC]
    public void RPC_WinningTeam(EInGameTeam team)
    {
        _winningTeam = team;
        ProduceScore();
    }
    
    public override void Tick()
    {
        
    }

    public override void Exit()
    {
        SummarizeResult();
        EventManager.Instance.OnRoundEnd -= EndCheck;
        EventManager.Instance.RoundStateExit();
    }
    
    // 결과 합산하기
    private void SummarizeResult()
    {
        PhotonPlayer myPlayer = PhotonNetwork.LocalPlayer;

        float totalDamage = (float)myPlayer.CustomProperties[EProperties.Damage.ToString()];
        float killCount = (float)myPlayer.CustomProperties[EProperties.Kill.ToString()];
        int time = (int)myPlayer.CustomProperties[EProperties.SurvivorTime.ToString()];
        
        _myDamage += totalDamage;
        _myKill += killCount;
        _mySurvivorTime += time;
        
        // 상태 초기화 
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
        {
            { EProperties.IsDead.ToString(), false },
            { EProperties.Kill.ToString(), _myKill },
            { EProperties.Damage.ToString(), _myDamage },
            { EProperties.SurvivorTime.ToString(), _mySurvivorTime }
        });
        
    }
    
}
