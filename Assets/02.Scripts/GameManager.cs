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
    private PhotonView _photonView;
    public PhotonView PhotonView => _photonView;
    private List<PhotonPlayer> _playerList = new List<PhotonPlayer>();
    
    [Header("게임 상태")]
    [SerializeField] private EGameState _currentGameState;
    public EGameState CurrentGameState => _currentGameState;
    
    [Header("게임 모드")]
    [SerializeField] private EGameMode _currentGameMode = EGameMode.Deathmatch;
    public EGameMode CurrentGameMode => _currentGameMode;
    private GM_IngameBase _currentModeHandler;
    
    public bool LastPlayer = false;
    private GameObject _myPlayer;
    
    [Header("부활 지점")]
    public Transform ResurrectPoint;                   // 부활 지점
    
    public event Action<PhotonPlayer> OnTimeCheck;
    public event Action OnGameStart;
    public event Action OnGameOver;

    protected override void Awake()
    {
        base.Awake();
        _photonView = GetComponent<PhotonView>();

        Debug.LogWarning($"현재 씬 이름 {SceneManager.GetActiveScene().name}");
        ClientManager.PlayBGM(SceneManager.GetActiveScene().name);

        if (_currentGameState == EGameState.Waiting)
        {
            _currentModeHandler = CreateModeHandler(EGameMode.Waiting);
            return;       
        }
        
        if(_currentGameState == EGameState.Tutorial)
        {
            _currentModeHandler = CreateModeHandler(EGameMode.Tutorial);
            return;
        }

        // 게임 모드 초기화
        SetupGameMode();

        TimeScaleSetting();
        EventManager.Instance.OnLoadFinished += Init;
        EventManager.Instance.OnPlayerLeft += PlayerLastCheck;
    }
    
    /// <summary>
    /// 게임 모드 설정 및 핸들러 생성
    /// </summary>
    private void SetupGameMode()
    {
        SetGameMode();
        
        _currentModeHandler = CreateModeHandler(_currentGameMode);
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        _playerList = new List<PhotonPlayer>(players);
        
        if (_currentModeHandler != null)
        {
            _currentModeHandler.Initialize(this, _playerList);
        }
    }
    private void SetGameMode()
    {
        if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.GameMode.ToString()) == false)
        {
            Debug.Log("GameMode is not set, using Deathmatch");
            _currentGameMode = EGameMode.Deathmatch;
            return;
        }
        
        EGameMode mode = (EGameMode)PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.GameMode.ToString()];
        _currentGameMode = mode;
        
        Debug.Log($"GameMode is set to {_currentGameMode}");
    }
    
    /// <summary>
    /// 게임 모드에 따라 핸들러 생성
    /// </summary>
    private GM_IngameBase CreateModeHandler(EGameMode mode)
    {
        switch (mode)
        {
            case EGameMode.Deathmatch:
                return new GM_IngameDeathmatch();
            case EGameMode.Infinite:
                return new GM_IngameInfinite();
            case EGameMode.Tutorial:
                return new GM_Tutorial();
            case EGameMode.Waiting:
                return new GM_Waiting();
            default:
                Debug.LogWarning($"Unknown game mode: {mode}, using Deathmatch");
                return new GM_IngameDeathmatch();
        }
    }

    // 처음부터 두명이서 시작할 경우 && 누군가 나갈 경우 플레이어 리스트 최신화
    private void PlayerLastCheck(PhotonPlayer player)
    {
        EventManager.Instance.TargetChanged();
        
        // 모드에 플레이어 나감 알림
        if (_currentModeHandler != null)
        {
            _currentModeHandler.OnPlayerLeft(player);
        }
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;       
        }
     
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        _playerList = new List<PhotonPlayer>(players);
        
        // 모드의 플레이어 리스트 업데이트
        if (_currentModeHandler != null)
        {
            _currentModeHandler.UpdatePlayerList();
        }
        
        CheckGameOverCondition();
    }
    
    private void Init()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        _photonView.RPC(nameof(RPC_GameStart), RpcTarget.All);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _photonView.RPC(nameof(RequestSpawn), RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    void RequestSpawn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("AirDropJet", transform.position, Quaternion.identity);
        }
    }

    // 게임 종료
    public void RequestGameOver()
    {
        _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
    }
    
    /// <summary>
    /// 타이머 종료 알림 (IngameTimer에서 호출)
    /// </summary>
    public void OnTimerExpired()
    {
        if (_currentModeHandler != null)
        {
            _currentModeHandler.OnTimerExpired();
            
            // 방장만 게임 종료 조건 체크
            if (PhotonNetwork.IsMasterClient)
            {
                CheckGameOverCondition();
            }
        }
    }
    
    [PunRPC]
    private void RPC_GameOver()
    {
        GameStateChange(EGameState.Result);
        OnGameOver?.Invoke();
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

        if (LastPlayer == false)
        {
            // 현재 살아있는 사람들 체크, 관전
            EventManager.Instance.TargetChanged();
        }
        
        // 플레이어가 죽었을 때 모드에 알림
        if ((bool)changedProps[EProperties.IsDead.ToString()])
        {
            if (_currentModeHandler != null)
            {
                _currentModeHandler.OnPlayerDead(targetPlayer);
            }
            
            OnTimeCheck?.Invoke(targetPlayer);
        }
        
        // 게임오버 체크 (방장만)
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        CheckGameOverCondition();
    }
    
    /// <summary>
    /// 게임 종료 조건 체크 (모드에 위임)
    /// </summary>
    private void CheckGameOverCondition()
    {
        Debug.Log("GameOverCondition");
        if (_currentModeHandler == null)
        {
            return;
        }
        
        if (_currentModeHandler.CheckGameOverCondition())
        {
            Debug.Log("GameOver");
            // 강제 종료 (플레이어들이 비정상적으로 종료했을 때 또는 게임이 바로 끝나야 할때 : 라스트 어택을 안거칠때)
            RequestGameOver();
        }
    }

    [PunRPC]
    public void RPC_GameStart()
    {
        OnGameStart?.Invoke();
        EventManager.Instance.ProfileInit();
        SceneManager.UnloadSceneAsync(ESceneList.StartSequence.ToString());
    }

    [PunRPC]
    public void RPC_LastPlayer()
    {
        LastPlayer = true;
    }
    public void GameStartSetting()
    { 
        EventManager.Instance.PlayerListUp();
        PlayerLastCheck(null);
        _myPlayer = GameObject.FindGameObjectWithTag("Player");
        GameStateChange(EGameState.Playing);
        
        // 모드에 게임 시작 알림
        if (_currentModeHandler != null)
        {
            _currentModeHandler.OnGameStart();
        }
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
        EventManager.Instance.OnLoadFinished -= Init;
        EventManager.Instance.OnPlayerLeft -= PlayerLastCheck;
    }
    
    private void OnDestroy()
    {
        if (_currentModeHandler != null)
        {
            _currentModeHandler.OnDestroy();
            _currentModeHandler = null;
        }
    }
}



