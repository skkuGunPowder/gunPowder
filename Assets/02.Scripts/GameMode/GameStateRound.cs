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
    // 팀별 집계 데이터
    private class TeamStatData
    {
        public int TotalHP;
        public float TotalKill;
        public float TotalDamage;
        public int InitialCount;
        public int MinActorNumber;

        public TeamStatData(int initialCount)
        {
            InitialCount = initialCount;
            MinActorNumber = int.MaxValue;
        }

        public float HPContribution => InitialCount > 0 ? (float)TotalHP / InitialCount : 0f;
    }

    private Dictionary<EInGameTeam, TeamScore> _roundTeamCount = new Dictionary<EInGameTeam, TeamScore>();
    private const int ROUND_SCORE_LIMIT = 5; // 라운드 설정

    // 팀별 집계 딕셔너리
    private Dictionary<EInGameTeam, TeamStatData> _teamStats = new Dictionary<EInGameTeam, TeamStatData>();
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
        _playerCount = 0;
        _teamStats.Clear();

        // 팀별 초기 인원 수 설정
        foreach (PhotonPlayer player in PhotonNetwork.PlayerList)
        {
            EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
            if (!_teamStats.ContainsKey(team))
            {
                _teamStats[team] = new TeamStatData(0);
            }
            _teamStats[team].InitialCount++;
        }
    }

    // 우승팀 체크 >> 방장이 체크 후 플레이어들에게 전달
    private void CheckWinningTeam(int hp, PhotonPlayer player)
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;

        _playerCount += 1;

        if (players.Length == 1)
        {
            _gameMode.RequestStateChange(EModeState.Over);   // 플레이어가 한명이라면 바로 종료
            return;
        }

        EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
        float kill = player.CustomProperties.ContainsKey(EProperties.Kill.ToString())
            ? (float)player.CustomProperties[EProperties.Kill.ToString()]
            : 0f;
        float damage = player.CustomProperties.ContainsKey(EProperties.Damage.ToString())
            ? (float)player.CustomProperties[EProperties.Damage.ToString()]
            : 0f;
        bool isDead = player.CustomProperties.ContainsKey(EProperties.IsDead.ToString()) &&
                      (bool)player.CustomProperties[EProperties.IsDead.ToString()];

        if (!_teamStats.ContainsKey(team))
        {
            _teamStats[team] = new TeamStatData(1);
        }

        _teamStats[team].TotalHP += hp;
        _teamStats[team].TotalKill += kill;
        _teamStats[team].TotalDamage += damage;

        // 생존 플레이어 중 최소 액터 번호 추적
        if (!isDead && player.ActorNumber < _teamStats[team].MinActorNumber)
        {
            _teamStats[team].MinActorNumber = player.ActorNumber;
        }

        if (players.Length != _playerCount)
        {
            return;
        }

        // 전원 수신 완료 → 팀 비교로 승자 결정
        // HP 기여도 내림차순 → 킬 내림차순 → 딜량 내림차순 → 최소 액터 오름차순
        EInGameTeam winTeam = EInGameTeam.Default;
        float bestHP = float.MinValue;
        float bestKill = float.MinValue;
        float bestDamage = float.MinValue;
        int bestMinActor = int.MaxValue;

        foreach (KeyValuePair<EInGameTeam, TeamStatData> kv in _teamStats)
        {
            TeamStatData stat = kv.Value;
            bool isBetterHP = stat.HPContribution > bestHP;
            bool isSameHP = stat.HPContribution == bestHP;
            bool isBetterKill = isSameHP && stat.TotalKill > bestKill;
            bool isSameKill = isSameHP && stat.TotalKill == bestKill;
            bool isBetterDamage = isSameKill && stat.TotalDamage > bestDamage;
            bool isSameDamage = isSameKill && stat.TotalDamage == bestDamage;
            bool isBetterActor = isSameDamage && stat.MinActorNumber < bestMinActor;

            if (isBetterHP || isBetterKill || isBetterDamage || isBetterActor)
            {
                bestHP = stat.HPContribution;
                bestKill = stat.TotalKill;
                bestDamage = stat.TotalDamage;
                bestMinActor = stat.MinActorNumber;
                winTeam = kv.Key;
            }
        }

        SetWinningTeam(winTeam);
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

        if (PhotonNetwork.IsMasterClient)
        {
            int score = _roundTeamCount[team].score;
            foreach (PhotonPlayer player in PhotonNetwork.PlayerList)
            {
                EInGameTeam playerTeam = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
                if (playerTeam == team)
                {
                    player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
                    {
                        { EProperties.RoundWins.ToString(), score }
                    });
                }
            }
        }
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

        // 마지막 라운드 우승 팀 기록
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
        {
            { ERoomProperties.LastRoundWinnerTeam.ToString(), (int)team }
        });

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
