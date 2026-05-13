using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStatePlaying : GameModeStateBase
{
    // 게임이 진행되는 동안 벌어지는 일들을 정리
    // lastpalyer체크, 현재 팀에 남아있는 인원 체크
    private bool _lastPlayer = false; 
    private bool _gameSet = false; // 동시 죽음 없애기 위함
    private Dictionary<EInGameTeam, int> _teamCount = new Dictionary<EInGameTeam, int>(); // 살아 있는 팀원 수 : 팀 / 팀원 수
    private Dictionary<EInGameTeam, int> _initialTeamCount = new Dictionary<EInGameTeam, int>(); // 게임 시작 시 팀 초기 인원 수
    private int _count = 0; // 모든 플레이어의 정보가 모였는지 확인
    private int _MaxCount = 0;
    
    private SecondTimer _timer;
    
    // 관전 해제
    private CameraController  _cameraController;
    public override void Initialize(GameModeBase gameMode)
    {
        base.Initialize(gameMode);
        if (_cameraController == null)
        {
            _cameraController = Camera.main.GetComponent<CameraController>();   
        }
        _teamCount = new Dictionary<EInGameTeam, int>();
        _teamCount.Clear();
    }

    private void Init()
    { 
        if (_cameraController == null)
        {
            _cameraController = Camera.main.GetComponent<CameraController>();   
        }
        
        EventManager.Instance.GameStart(); // 게임 시작 321
        EventManager.Instance.ProfileInit(); // 프로필 리프레시
        
        // 시간 초
        
        // 팀원 추가
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        
        foreach (PhotonPlayer player in players)
        {
            EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
            
            Debug.LogWarning($"{player.NickName}이 팀은 {team.ToString()}입니다.");
            _teamCount.TryAdd(team, 0);
            _teamCount[team]++;
            _initialTeamCount.TryAdd(team, 0);
            _initialTeamCount[team]++;
            
            Debug.LogWarning($" teamcount {_teamCount[team].ToString()} 설정합니다., init {_initialTeamCount[team].ToString()}를 설정합니다.");
        }
        
        _MaxCount = players.Length;
        
        // 시간 설정
        int playTime = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.PlayTime.ToString()].ToString()) * 60;
        
        _timer = new SecondTimer(playTime, () => { if(PhotonNetwork.IsMasterClient) OnTimeOver(); },
            (sec) => EventManager.Instance.TimerUpdate(sec)
        );
    }

    public override void Enter()
    {
        Init();
        GameManager.Instance.GameStateChange(EGameState.Waiting); // 카운트다운 중 Waiting 유지
        
        EventManager.Instance.OnTimeCheck += OnPlayerDead;
        EventManager.Instance.OnPlayerLeft += OnPlayerLeft;
        EventManager.Instance.OnLastDieComplete += GameResultCheck;
        EventManager.Instance.OnGameStateChangeCheck += GameStateChangeCheck;

        OnPlayerLeft(); // 플레이어가 게임 시작 전에 나간경우를 체크하기 위함
    }

    private void OnPlayerLeft(PhotonPlayer player = null)
    {
        // 카메라 컨트롤러 : 관전을 위해 모든 플레이어 찾기
        EventManager.Instance.TargetChanged(); 
        
        if (player != null)
        {
            bool isDead = (bool)player.CustomProperties[EProperties.IsDead.ToString()];
            
            if (isDead)
            {
                return;
            }
            
            EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
            _teamCount[team]--;
            _count++;   // 플레이어가 죽지 않았는데 나갔다면 죽은 카운트 ++
        }

        if (PhotonNetwork.IsMasterClient == false)
        {
            return;       
        }

        // 게임 상황 체크
        GameOverToPlayerLeft();
    }

    private void OnPlayerDead(PhotonPlayer player)
    {
        if (_gameSet) // 게임이 끝났다면 플레이어를 죽음 상태로 보내지 않음
        {
            return;
        }
        
        // 게임오버 체크
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        string teamKey = EProperties.Team.ToString();
        if (!player.CustomProperties.ContainsKey(teamKey))
        {
            Debug.LogError($"[OnPlayerDead] {player.NickName}의 Team 커스텀 프로퍼티를 찾을 수 없습니다.");
            return;
        }

        EInGameTeam team = (EInGameTeam)player.CustomProperties[teamKey];
        _teamCount[team]--;

        if (_lastPlayer)
        {
            if (LastTeamCheck() < 1)    // 팀원이 살아있는 팀이 1개 이상인지
            {
                return;
            }

            if (LastAttackCheck(team) == false)     // 팀원이 살아있음 - 일반 사망 처리만
            {
                _photonView.RPC(nameof(RPC_RequestPlayerDie), player, false);
                return;
            }

            // 팀의 마지막 플레이어 - 게임 종료
            SetGameSet();
            GameManager.Instance.GameStateChange(EGameState.Result);
            _photonView.RPC(nameof(RPC_RequestPlayerDie), player, true);
            return;
        }
        
        
        // 플레이어 상태 변경
        _photonView.RPC(nameof(RPC_RequestPlayerDie), player, false);
        
        PlayerDeadCheck();
    }

    private void GameStateChangeCheck(PhotonPlayer player)
    {
        _count++;
        if (_gameMode is BattleMode battleMode)
        {
            battleMode.DeathOrderQueue.Enqueue(player);
        }

        if (_count >= _MaxCount)
        {
            RequestStateChange();
            _timer.Destroy();
        }
    }
    // 막타 가능 상태 체크
    private void PlayerDeadCheck()
    {
        int notDead = 0;

        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        
        foreach (PhotonPlayer p in players)
        {
            bool isDead = p.CustomProperties.ContainsKey(EProperties.IsDead.ToString()) &&
                          (bool)p.CustomProperties[EProperties.IsDead.ToString()];
            
            if (isDead == false)
            {
                notDead++;
            }
        }
        
        if (_lastPlayer == false && LastTeamCheck() == 2) // LastPlayer가 아닌데 팀이 2팀일 때 => 정상적 플레이
        {
            _lastPlayer = true;
            _photonView.RPC(nameof(RPC_LastPlayer), RpcTarget.Others);
            return;
        }
        
        // 플레이어가 두명 남았는가?
        if (_lastPlayer == false && notDead == 2)   // lastPlayer가 아닌데 살아 있는 플레이어가 2명일 때
        {
            _lastPlayer = true;
            _photonView.RPC(nameof(RPC_LastPlayer), RpcTarget.Others);
        }
        
    }
    
    
    // 플레이어가 나가서 게임이 끝나는 경우
    private void GameOverToPlayerLeft() 
    {
        int notDead = 0;
        
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        
        // 나가서 혼자인 경우 : 플레이어가 한명인 경우는 무조건 종료
        if (players.Length == 1)
        {
            _gameMode.GameOver();
            return;
        }

        if (RoomTeamCheck(players))
        {
            _gameMode.GameOver();
            return;
        }
        
        foreach (PhotonPlayer p in players)
        {
            bool isDead = p.CustomProperties.ContainsKey(EProperties.IsDead.ToString()) &&
                          (bool)p.CustomProperties[EProperties.IsDead.ToString()];
            
            if (isDead == false)
            {
                notDead++;
            }
        }
        
        if (notDead == 0)   // 나가서 살아있는 사람이 없을 때
        {
            GameResultCheck();
            return;
        }
        
        // 2명 이상인데 살아있는 사람이 1명일 때
        if (_lastPlayer == false && notDead == 1)
        {
            GameResultCheck();
            return;
        }

        if (_lastPlayer && notDead == 1)
        {
            GameResultCheck();
            return;
        }
        
        // 나갔는데 살아있는 팀이 한팀 뿐일 때
        if (LastTeamCheck() <= 1)
        {
            GameResultCheck();
            return;
        }
        
        // 팀이 2팀이 되었을 때 LastPlayer = true
        if (_lastPlayer == false && LastTeamCheck() == 2)
        {
            _lastPlayer = true;
            _photonView.RPC(nameof(RPC_LastPlayer), RpcTarget.Others);
        }
        
    }

    private bool RoomTeamCheck(PhotonPlayer[] players)
    {
        EInGameTeam team = (EInGameTeam)players[0].CustomProperties[EProperties.Team.ToString()];

        for (int i = 1; i < players.Length; i++)
        {
            EInGameTeam other = (EInGameTeam)players[i].CustomProperties[EProperties.Team.ToString()];
            
            if (team != other)
            {
                return false;
            }
        }

        return true;
    }
    
        
    /// <summary>
    /// 게임 종료 체크를 위한 함수들
    /// 팀 개수 체크, 막타 체크
    /// </summary>
   
    // return 살아있는 팀원이 있는 팀 개수
    private int LastTeamCheck()
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
    private bool LastAttackCheck(EInGameTeam team)
    {
        return _teamCount[team] <= 0;
    }

    [PunRPC]
    private void RPC_LastPlayer() // 혹시 방장이 나가서 최신화가 안될 경우를 대비
    {
        _lastPlayer = true;
    }
    
    /// <summary>
    /// 플레이어 죽음 상태 변경하기
    /// 막타 연출이 나와야 할 경우 LastDieState로 그게 아니라면 DieState 변경함
    /// DieState의 경우 관전 시작 , LastDieState의 경우 막타 연출 시작
    /// </summary>
    [PunRPC] // [RPC] [PunRPC]
    private void RPC_RequestPlayerDie(bool isLastPlayer)
    {
        PlayerFSM fsm = _gameMode.MyPlayer.GetComponent<PlayerFSM>();
        PlayerStat stat = _gameMode.MyPlayer.GetComponent<PlayerStat>();
        
        if (isLastPlayer)
        {
            fsm.SyncStateChange<PlayerLastDieState>();
        }
        else
        {
            fsm.SyncStateChange<PlayerDieState>();
        }
        
        // 방장에게 시간 체크
        _photonView.RPC(nameof(RPC_RequestSurvivorTime), RpcTarget.MasterClient);
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
        {
            { EProperties.IsDead.ToString(), true },
            { EProperties.Kill.ToString(), stat.TotalKillCount },
            { EProperties.Damage.ToString(), stat.TotalDamage },
            { EProperties.HP.ToString(), 0 }
        });
    }
    
    private void OnTimeOver()
    {
        _photonView.RPC(nameof(RPC_TimeOver), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_TimeOver()
    {
        GameManager.Instance.GameStateChange(EGameState.Result);
        SetGameSet();
        GameResultCheck();
    }

    // 타임 오버가 되었을 때 로컬 플레이어가 살아있는 경우 나의 프로퍼티를 보낸다.
    public void GameResultCheck()
    {
        PhotonPlayer player = PhotonNetwork.LocalPlayer;
        if ((bool)player.CustomProperties[EProperties.IsDead.ToString()])
        {
            return;
        }

        PlayerStat stat = _gameMode.MyPlayer.GetComponent<PlayerStat>();

        Hashtable properties = new Hashtable()
        {
            {EProperties.IsDead.ToString(), true},
            {EProperties.Kill.ToString(), stat.TotalKillCount},
            {EProperties.Damage.ToString(), stat.TotalDamage},
            {EProperties.HP.ToString(), stat.CurrentHP}
        };

        // 방장의 타이머 기준으로 SurvivorTime 설정 요청 (방장/비방장 공통)
        _photonView.RPC(nameof(RPC_RequestSurvivorTime), RpcTarget.MasterClient);

        player.SetCustomProperties(properties);
    }

    [PunRPC]
    private void RPC_RequestSurvivorTime(PhotonMessageInfo info)
    {
        SurvivorTimeCheck(info.Sender);
    }

    private void SurvivorTimeCheck(PhotonPlayer player)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        int playtime = _timer.GetSurviveTime();
        
        Hashtable hash = new Hashtable() 
        {
            {EProperties.SurvivorTime.ToString(), playtime} 
        };
        
        player.SetCustomProperties(hash);
    }
    
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

    // LastDie로 죽었을 때 변화
    private void RequestStateChange()
    {
        if (PhotonNetwork.IsMasterClient && _gameMode is BattleMode battleMode)
        {
            // 팀별 집계 딕셔너리 초기화
            Dictionary<EInGameTeam, TeamStatData> teamStats = new Dictionary<EInGameTeam, TeamStatData>();
            foreach (KeyValuePair<EInGameTeam, int> kv in _initialTeamCount)
            {
                teamStats[kv.Key] = new TeamStatData(kv.Value);
            }

            // 팀별 플레이어 목록
            Dictionary<EInGameTeam, List<PhotonPlayer>> teamPlayers = new Dictionary<EInGameTeam, List<PhotonPlayer>>();
            foreach (EInGameTeam team in teamStats.Keys)
            {
                teamPlayers[team] = new List<PhotonPlayer>();
            }

            // 현재 플레이어 데이터 수집
            foreach (PhotonPlayer player in PhotonNetwork.PlayerList)
            {
                EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
                if (!teamStats.ContainsKey(team))
                {
                    continue;
                }

                teamPlayers[team].Add(player);

                int hp = player.CustomProperties.ContainsKey(EProperties.HP.ToString())
                    ? (int)player.CustomProperties[EProperties.HP.ToString()]
                    : 0;
                float kill = player.CustomProperties.ContainsKey(EProperties.Kill.ToString())
                    ? (float)player.CustomProperties[EProperties.Kill.ToString()]
                    : 0f;
                float damage = player.CustomProperties.ContainsKey(EProperties.Damage.ToString())
                    ? (float)player.CustomProperties[EProperties.Damage.ToString()]
                    : 0f;
                bool isDead = player.CustomProperties.ContainsKey(EProperties.IsDead.ToString()) &&
                              (bool)player.CustomProperties[EProperties.IsDead.ToString()];

                teamStats[team].TotalHP += hp;
                teamStats[team].TotalKill += kill;
                teamStats[team].TotalDamage += damage;

                // 생존 플레이어 중 최소 액터 번호 추적
                if (!isDead && player.ActorNumber < teamStats[team].MinActorNumber)
                {
                    teamStats[team].MinActorNumber = player.ActorNumber;
                }
            }

            // 팀 정렬: HP 기여도 오름차순 → 킬 오름차순 → 딜량 오름차순 → 최소 액터 내림차순
            List<EInGameTeam> sortedTeams = new List<EInGameTeam>(teamStats.Keys);
            sortedTeams.Sort((a, b) =>
            {
                float hpA = teamStats[a].HPContribution;
                float hpB = teamStats[b].HPContribution;
                if (hpA != hpB)
                {
                    return hpA.CompareTo(hpB);
                }

                float killA = teamStats[a].TotalKill;
                float killB = teamStats[b].TotalKill;
                if (killA != killB)
                {
                    return killA.CompareTo(killB);
                }

                float damageA = teamStats[a].TotalDamage;
                float damageB = teamStats[b].TotalDamage;
                if (damageA != damageB)
                {
                    return damageA.CompareTo(damageB);
                }

                // 최소 액터 번호 내림차순: 낮은 번호 팀이 나중에 픽(유리)
                return teamStats[b].MinActorNumber.CompareTo(teamStats[a].MinActorNumber);
            });

            // 팀 내 개인 정렬: 킬 오름차순 → 딜량 오름차순 → 액터 번호 오름차순
            List<PhotonPlayer> finalOrder = new List<PhotonPlayer>();
            foreach (EInGameTeam team in sortedTeams)
            {
                List<PhotonPlayer> players = teamPlayers[team];
                players.Sort((a, b) =>
                {
                    float killA = a.CustomProperties.ContainsKey(EProperties.Kill.ToString())
                        ? (float)a.CustomProperties[EProperties.Kill.ToString()]
                        : 0f;
                    float killB = b.CustomProperties.ContainsKey(EProperties.Kill.ToString())
                        ? (float)b.CustomProperties[EProperties.Kill.ToString()]
                        : 0f;
                    if (killA != killB)
                    {
                        return killA.CompareTo(killB);
                    }

                    float damageA = a.CustomProperties.ContainsKey(EProperties.Damage.ToString())
                        ? (float)a.CustomProperties[EProperties.Damage.ToString()]
                        : 0f;
                    float damageB = b.CustomProperties.ContainsKey(EProperties.Damage.ToString())
                        ? (float)b.CustomProperties[EProperties.Damage.ToString()]
                        : 0f;
                    if (damageA != damageB)
                    {
                        return damageA.CompareTo(damageB);
                    }

                    return a.ActorNumber.CompareTo(b.ActorNumber);
                });

                finalOrder.AddRange(players);
            }

            int[] actorNumbers = new int[finalOrder.Count];
            for (int i = 0; i < finalOrder.Count; i++)
            {
                actorNumbers[i] = finalOrder[i].ActorNumber;
            }
            _photonView.RPC(nameof(RPC_SyncDeathOrder), RpcTarget.Others, actorNumbers);

            // 마스터 클라이언트 로컬 큐도 갱신
            battleMode.DeathOrderQueue.Clear();
            foreach (PhotonPlayer player in finalOrder)
            {
                battleMode.DeathOrderQueue.Enqueue(player);
            }
        }
        _gameMode.RequestStateChange(EModeState.Round);
    }

    [PunRPC]
    private void RPC_SyncDeathOrder(int[] actorNumbers)
    {
        if (_gameMode is BattleMode battleMode)
        {
            battleMode.DeathOrderQueue.Clear();
            
            foreach (int actorNumber in actorNumbers)
            {
                PhotonPlayer player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
                if (player != null)
                {
                    battleMode.DeathOrderQueue.Enqueue(player);
                }
            }
        }
    }
    
    private void SetGameSet()
    {
        _gameSet = true;
        _photonView.RPC(nameof(RPC_SetGameSet), RpcTarget.Others);
    }

    [PunRPC]
    private void RPC_SetGameSet()
    {
        _gameSet = true;
    }

    public override void Tick()
    {
        // 타이머 
        _timer?.Tick(Time.deltaTime);   
    }

    public override void Exit()
    {
        //관전이 켜져있다면 관전 해제
        _cameraController.CancelObserve();
        
        // 플레이어 GP를 RoomStatManager에 저장
        PlayerStat stat = _gameMode.MyPlayer.GetComponent<PlayerStat>();
        RoomStatManager.Instance.SetGunpowder(stat.CurrentGP);
        GameManager.Instance.GameStateChange(EGameState.Waiting); // Waiting 상태로 복귀
        _lastPlayer = false;
        _gameSet = false;
        _teamCount.Clear();
        _initialTeamCount.Clear();
        _count = 0;
        EventManager.Instance.OnLastDieComplete -= GameResultCheck;
        EventManager.Instance.OnPlayerLeft -= OnPlayerLeft;
        EventManager.Instance.OnTimeCheck -= OnPlayerDead;
        EventManager.Instance.OnGameStateChangeCheck -= GameStateChangeCheck;
    }
}