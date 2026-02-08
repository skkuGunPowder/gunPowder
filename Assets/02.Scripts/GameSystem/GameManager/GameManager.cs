using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonPlayer = Photon.Realtime.Player;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class GameManager : PhotonSingleton<GameManager> 
{
    /// <summary>
    /// 인게임 시작과 끝을 담당 (게임 시작, 게임 종료)
    /// 플레이어들이 나갔을 때 or 모두 죽었을 때 게임을 종료 시킴
    /// 1. 게임 시작 끝
    /// 2. 플레이어 나갔을 때 실행될 함수
    /// 3. 죽음에 대한 함수
    /// 4. RPC
    /// </summary>
    private PhotonView _photonView;
    
    [Header("현재 게임 상태")]
    [SerializeField] private EGameState _currentGameState;
    public EGameState CurrentGameState => _currentGameState;

    [Header("게임 모드")]
    [SerializeField] private GameModeBase GameMode;  // 부활 지점
    
    [Header("게임매니저 컴포넌트")]
    // 플레이어 인원수 체크 및 팀 체크
    private TeamTracker _teamTracker;
    // 라운드 
    
    private bool _lastPlayer = false; // 마지막 연출 실행 여부
    
    private GameObject _myPlayer;   // 내 로컬 플레이어 (게임 종료시 : 결과에 필요한 정보 수집을 위함)

    public event Action<PhotonPlayer> OnTimeCheck;  // 죽은 사람 시간 체크

    protected override void Awake()
    {
        base.Awake();
        _photonView = GetComponent<PhotonView>();

        Debug.LogWarning($"현재 씬 이름 {SceneManager.GetActiveScene().name}");
        ClientManager.PlayBGM(SceneManager.GetActiveScene().name);

        if (_currentGameState == EGameState.Waiting || _currentGameState == EGameState.Tutorial)
        {
            return;
        }

        TimeScaleSetting();
        EventManager.Instance.OnLoadFinished += RequestGameStart;
        EventManager.Instance.OnPlayerLeft += PlayerLeft;
    }

    // 게임 시작 (모든 플레이어들이 로드가 끝났을 때 : OnLoadFinished)
    private void RequestGameStart()
    {
        // 방장만 게임을 시작할 수 있음
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        _photonView.RPC(nameof(RPC_GameStart), RpcTarget.All);
    }

    public void GameStartSetting()
    { 
        EventManager.Instance.PlayerListUp();
        GameStateChange(EGameState.Playing);
        PlayerLeft();   // 게임 시작 플레이어 체크
        _myPlayer = GameObject.FindGameObjectWithTag("Player");  // 내 로컬 플레이어 찾기
    }
    
    // 게임 종료
    public void RequestGameOver()
    {
        _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
    }
    
    /// <summary>
    /// 플레이어가 나갔을 때, 그 플레이어가 죽지 않았다면 팀원 숫자에서 빼줌
    /// </summary>
    private void PlayerLeft(PhotonPlayer player = null)
    {
        if (_currentGameState != EGameState.Playing)
        {
            return;
        }
        
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
            _teamTracker.SubPlayer(team);
        }

        if (PhotonNetwork.IsMasterClient == false)
        {
            return;       
        }

        // 게임 상황 체크
        GameOverToPlayerLeft();
    }
    
    // 플레이어가 나가서 게임이 끝나는 경우
    private void GameOverToPlayerLeft() 
    {
        int notDead = 0;
        
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        
        // 나가서 혼자인 경우 : 플레이어가 한명인 경우는 무조건 종료
        if (players.Length == 1)
        {
            _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
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
            _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
            return;
        }
        
        // 2명 이상인데 살아있는 사람이 1명일 때
        if (_lastPlayer == false && notDead == 1)
        {
            _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
            return;
        }

        if (_lastPlayer && notDead == 1)
        {
            _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
            return;
        }
        
        // 나갔는데 살아있는 팀이 한팀 뿐일 때
        if (_teamTracker.LastTeamCheck() <= 1)
        {
            _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
            return;
        }
        
        // 팀이 2팀이 되었을 때 LastPlayer = true
        if (_lastPlayer == false && _teamTracker.LastTeamCheck() == 2)
        {
            _lastPlayer = true;
            _photonView.RPC(nameof(RPC_LastPlayer), RpcTarget.Others);
        }
        
    }
    
    /// <summary>
    /// 프로퍼티가 바뀌었을 때 호출되는 함수
    /// 플레이어가 죽을 때마다 죽은 플레이어들 체크하기
    /// </summary>
    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer ,Hashtable changedProps)
    {
        if (_currentGameState == EGameState.Waiting || _currentGameState == EGameState.GameOver)
        {
            return;
        }

        if (!changedProps.ContainsKey(EProperties.IsDead.ToString()) || changedProps[EProperties.IsDead.ToString()] == null)
        {
            return;
        }
        
        // 죽은 사람 팀원 수에서 빼기
        EInGameTeam team = (EInGameTeam)targetPlayer.CustomProperties[EProperties.Team.ToString()];
        _teamTracker.SubPlayer(team);
        
        // 게임오버 체크
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        if ((bool)changedProps[EProperties.IsDead.ToString()])
        {
            // 죽은 사람 죽은 시간 체크 후 저장
            OnTimeCheck?.Invoke(targetPlayer);
        }

        if (_lastPlayer)
        {
            if (_teamTracker.LastTeamCheck() < 1)    // 팀원이 살아있는 팀이 1개 이상인지
            {
                return;
            }
            
            bool end;
            
            if (_teamTracker.LastAttackCheck(team) == false)     //죽은 플레이어가 그 팀의 마지막인가? 
            {
                end = false;
            }
            else
            {
                end = true;
            }
        
            // 플레이어 상태 변경
            _photonView.RPC(nameof(RPC_RequestPlayerDie), targetPlayer, end);
            return;
        }
        
        // 플레이어 상태 변경
        _photonView.RPC(nameof(RPC_RequestPlayerDie), targetPlayer, false);

        // 막타 상태 체크
        PlayerDeadCheck();
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
        
        if (_lastPlayer == false && _teamTracker.LastTeamCheck() == 2) // LastPlayer가 아닌데 팀이 2팀일 때 => 정상적 플레이
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
    
    // 타임 오버가 되었을 때 로컬 플레이어가 살아있는 경우 나의 프로퍼티를 보낸다.
    public void GameResultCheck()
    {
        PhotonPlayer player = PhotonNetwork.LocalPlayer;
        if ((bool)player.CustomProperties[EProperties.IsDead.ToString()])
        {
            return;
        }
        
        PlayerStat stat = _myPlayer.GetComponent<PlayerStat>();
        
        Hashtable properties = new Hashtable()
        {
            {EProperties.IsDead.ToString(), true},
            {EProperties.Kill.ToString(), stat.TotalKillCount},
            {EProperties.Damage.ToString(), stat.TotalDamage}
        };

        if (PhotonNetwork.IsMasterClient)
        {
            OnTimeCheck?.Invoke(player);
        }
        
        player.SetCustomProperties(properties);
    }
    
    //게임 상태 변경, 게임 상태에 따라 타임 스케일 조정
    public void GameStateChange(EGameState state)
    {
        _currentGameState = state;
        TimeScaleSetting();
    }
    
    public void TimeScaleSetting()
    {
        if (_currentGameState == EGameState.Ready || _currentGameState == EGameState.Ultimate)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
    
    
    public Transform GetResurrectPoint()
    {
        return GameMode.GetResurrectPoint();
    }
    
    /// <summary>
    /// 플레이어 죽음 상태 변경하기
    /// 막타 연출이 나와야 할 경우 LastDieState로 그게 아니라면 DieState 변경함
    /// DieState의 경우 관전 시작 , LastDieState의 경우 막타 연출 시작
    /// </summary>
    [PunRPC] // [RPC] [PunRPC]
    private void RPC_RequestPlayerDie(bool isLastPlayer)
    {
        PlayerFSM fsm = _myPlayer.GetComponent<PlayerFSM>();

        if (isLastPlayer)
        {
            fsm.SyncStateChange<PlayerLastDieState>();
        }
        else
        {
            fsm.SyncStateChange<PlayerDieState>();
        }
    }
    
    [PunRPC]
    public void RPC_GameStart()
    {
        EventManager.Instance.GameStart(); // 게임 시작 321
        EventManager.Instance.ProfileInit(); // 프로필 리프레시
        _teamTracker.Init();
        SceneManager.UnloadSceneAsync(ESceneList.StartSequence.ToString()); // 연출씬 제거
    }

    [PunRPC]
    private void RPC_LastPlayer() // 혹시 방장이 나가서 최신화가 안될 경우를 대비
    {
        _lastPlayer = true;
    }
    
    [PunRPC]
    private void RPC_GameOver()
    {
        GameStateChange(EGameState.Result);
        EventManager.Instance.OnPlayerLeft -= PlayerLeft;
        EventManager.Instance.GameOver();
    }
    
    public override void OnDisable()
    { 
        base.OnDisable();
        EventManager.Instance.OnLoadFinished -= RequestGameStart;
    }
}



