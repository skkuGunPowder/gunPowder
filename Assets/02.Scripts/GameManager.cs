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
    private List<PhotonPlayer> _playerList = new List<PhotonPlayer>();
    
    [SerializeField] private EGameState _currentGameState;
    public EGameState CurrentGameState => _currentGameState;
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

        if (_currentGameState == EGameState.Waiting || _currentGameState == EGameState.Tutorial)
        {
            return;
        }

        TimeScaleSetting();
        EventManager.Instance.OnLoadFinished += Init;
    }

    // 처음부터 두명이서 시작할 경우
    private void PlayerLastCheck()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        _playerList = new List<PhotonPlayer>(players);
        
        if (_playerList.Count == 2)
        {
            LastPlayer = true; 
        }
    }
    
    private void Init()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        _photonView.RPC(nameof(RPC_GameStart), RpcTarget.All);
    }
    
    // 게임 종료
    public void RequestGameOver()
    {
        _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
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

        if (!changedProps.ContainsKey(EProperties.IsDead.ToString()) && changedProps[EProperties.IsDead.ToString()] == null)
        {
            return;
        }
        
        // 현재 살아있는 사람들 체크, 관전
        EventManager.Instance.TargetChanged();
        // 게임오버 체크
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        if ((bool)changedProps[EProperties.IsDead.ToString()])
        {
            OnTimeCheck?.Invoke(targetPlayer);
        }   
        
        PlayerDeadCheck();
    }
    
    // 캐릭터들 사망 체크하기 = 방장만
    private void PlayerDeadCheck()
    {
        int notDead = 0;
        
        foreach (PhotonPlayer p in _playerList)
        {
            bool isDead = p.CustomProperties.ContainsKey(EProperties.IsDead.ToString()) && (bool)p.CustomProperties[EProperties.IsDead.ToString()];
            if (isDead == false)
            {
                notDead++;
            }
        }

        // 플레이어가 두명 남았는가?
        if (LastPlayer == false && notDead == 2)
        {
            _photonView.RPC(nameof(RPC_LastPlayer), RpcTarget.All);
        }
    }

    [PunRPC]
    public void RPC_GameStart()
    {
        OnGameStart?.Invoke();
        EventManager.Instance.ProfileInit();
        _myPlayer = GameObject.FindGameObjectWithTag("Player");
        SceneManager.UnloadSceneAsync(ESceneList.StartSequence.ToString());
    }

    [PunRPC]
    private void RPC_LastPlayer()
    {
        LastPlayer = true;
    }
    public void GameStartSetting()
    { 
        EventManager.Instance.PlayerListUp();
        EventManager.Instance.TargetChanged();
        GameStateChange(EGameState.Playing);
        PlayerLastCheck();
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
    }
    
    private void OnDestroy()
    {
    }
}



