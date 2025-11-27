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
    private Dictionary<EInGameTeam, int> _teamCount = new Dictionary<EInGameTeam, int>(); // 현재 플레이어 팀 상태
    private PhotonView _photonView;
    [SerializeField] private EGameState _currentGameState;
    public EGameState CurrentGameState => _currentGameState;
    public bool LastPlayer = false;
    private GameObject _myPlayer;
    
    [Header("부활 지점")]
    public Transform ResurrectPoint;                   // 부활 지점
    public event Action<PhotonPlayer> OnTimeCheck;

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
        EventManager.Instance.OnLoadFinished += Init;
        EventManager.Instance.OnPlayerLeft += PlayerLastCheck;
    }

    // 처음부터 두명이서 시작할 경우 && 누군가 나갈 경우 플레이어 리스트 최신화
    private void PlayerLastCheck(PhotonPlayer player)
    {
        if (_currentGameState != EGameState.Playing)
        {
            return;
        }
        
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
        }

        if (PhotonNetwork.IsMasterClient == false)
        {
            return;       
        }

        GameOverToPlayerLeft();
    }
    
    private void Init()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        _photonView.RPC(nameof(RPC_GameStart), RpcTarget.All);
    }

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Alpha0))
    //     {
    //         _photonView.RPC(nameof(RequestSpawn), RpcTarget.MasterClient);
    //     }
    // }

    // [PunRPC]
    // void RequestSpawn()
    // {
    //     if (PhotonNetwork.IsMasterClient)
    //     {
    //         PhotonNetwork.Instantiate("AirDropJet", transform.position, Quaternion.identity);
    //         Debug.Log("마스터가 요청받아 오브젝트를 생성했습니다.");
    //     }
    // }

    // 게임 종료
    public void RequestGameOver()
    {
        _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
        
        Debug.Log("GameOver");
    }
    
    [PunRPC]
    private void RPC_GameOver()
    {
        Debug.Log("GameOver");
        GameStateChange(EGameState.Result);
        EventManager.Instance.OnPlayerLeft -= PlayerLastCheck;
        EventManager.Instance.GameOver();
    }
    
    // 프로퍼티가 바뀌었을 때 호출되는 함수
    // 플레이어가 죽을 때마다 죽은 플레이어들 체크하기
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
        
        EInGameTeam team = (EInGameTeam)targetPlayer.CustomProperties[EProperties.Team.ToString()];
        _teamCount[team]--;
        
        // 게임오버 체크
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        if ((bool)changedProps[EProperties.IsDead.ToString()])
        {
            OnTimeCheck?.Invoke(targetPlayer);
        }

        Debug.Log($"{targetPlayer.NickName}이 죽었습니다. 지금 남은 팀 {LastTeamCheck()}팀");
        if (LastPlayer)
        {
            if (LastTeamCheck() < 1)
            {
                return;
            }
            
            bool end;
            
            if (LastAttackCheck(team) == false)
            {
                end = false;
            }
            else
            {
                end = true;
            }
            
            Debug.Log($"{targetPlayer.NickName}이 죽습니다. 라스트 어택 상태 {end}");
            _photonView.RPC(nameof(RPC_RequestPlayerDie), targetPlayer, end);
            return;
        }
        
        _photonView.RPC(nameof(RPC_RequestPlayerDie), targetPlayer, false);

        
        PlayerDeadCheck();
    }
    
    // 캐릭터들 사망 체크하기 = 방장만
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
        
        if (LastPlayer == false && LastTeamCheck() == 2) // LastPlayer가 아닌데 팀이 2팀일 때 => 정상적 플레이
        {
            LastPlayer = true;
            _photonView.RPC(nameof(RPC_LastPlayer), RpcTarget.Others);
            return;
        }
        
        // 플레이어가 두명 남았는가?
        if (LastPlayer == false && notDead == 2)
        {
            LastPlayer = true;
            _photonView.RPC(nameof(RPC_LastPlayer), RpcTarget.Others);
        }
        
    }

    private void GameOverToPlayerLeft() // 플레이어가 나가서 게임이 끝나는 경우
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

        // 나가서 혼자인 경우
        if (players.Length == 1)
        {
            _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
            return;
        }
        
        if (notDead == 0)   // 나가서 살아있는 사람이 없을 때
        {
            _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
            return;
        }
        
        // 2명 이상인데 살아있는 사람이 1명일 때
        if (LastPlayer == false && notDead == 1)
        {
            _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
            return;
        }

        if (LastPlayer && notDead == 1)
        {
            _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
            return;
        }
        
        // 나갔는데 살아있는 팀이 한팀 뿐일 때
        if (LastTeamCheck() <= 1)
        {
            _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
            return;
        }
        
        // 팀이 2팀이 되었을 때 LastPlayer = true
        if (LastPlayer == false && LastTeamCheck() == 2)
        {
            LastPlayer = true;
            _photonView.RPC(nameof(RPC_LastPlayer), RpcTarget.Others);
        }
        
    }
    
    [PunRPC]
    public void RPC_GameStart()
    {
        EventManager.Instance.GameStart(); // 게임 시작 321
        EventManager.Instance.ProfileInit(); // 프로필 리프레시
        TeamSetting(); // 팀개수 팀원 수 체크
        SceneManager.UnloadSceneAsync(ESceneList.StartSequence.ToString()); // 연출씬 제거
    }

    [PunRPC]
    private void RPC_LastPlayer() // 혹시 방장이 나가서 최신화가 안될 경우를 대비
    {
        LastPlayer = true;
    }
    
    public void GameStartSetting()
    { 
        EventManager.Instance.PlayerListUp();
        GameStateChange(EGameState.Playing);
        PlayerLastCheck(null);
        _myPlayer = GameObject.FindGameObjectWithTag("Player");
    }
    
    // 타임 오버가 되었을 때 로컬로 나의 프로퍼티를 보낸다.
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
    
    public override void OnDisable()
    { 
        base.OnDisable();
        EventManager.Instance.OnLoadFinished -= Init;
    }

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
    
    private int LastTeamCheck() // 살아있는 팀원이 1명 이상 있는지 체크 -> 2팀이라면 lastPlayer = true
    {
        int count = 0;
        
        foreach (int value in _teamCount.Values)
        {
            if(value > 0) 
            {
                count++; // ( value > 0)
            }
        }
        
        return count;
    }
    // 플레이어가 죽었을 때 -> 막타로 가야하는지 체크
    private bool LastAttackCheck(EInGameTeam team)
    {
        return _teamCount[team] <= 0;
    }

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
}



