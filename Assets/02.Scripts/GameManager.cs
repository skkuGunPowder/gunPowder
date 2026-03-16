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
    [Header("현재 게임 상태")]
    [SerializeField] private EGameState _currentGameState;
    public EGameState CurrentGameState => _currentGameState;

    [Header("게임 모드")]
    [SerializeField] private GameModeBase GameMode;  // 부활 지점
    
    private PhotonView _photonView;


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
    }

    // 게임 시작 (모든 플레이어들이 로드가 끝났을 때 : OnLoadFinished)
    public void RequestGameStart()
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
        GameStateChange(EGameState.Playing);
        EventManager.Instance.PlayerListUp();
        // 카메라 컨트롤러 : 관전을 위해 모든 플레이어 찾기
        EventManager.Instance.TargetChanged(); 
    }
    
    // 게임 종료
    public void RequestGameOver()
    {
        _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
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
        
        if ((bool)changedProps[EProperties.IsDead.ToString()])
        {
            // 죽은 사람 죽은 시간 체크 후 저장
            EventManager.Instance.TimeCheck(targetPlayer);
        }
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
    
    [PunRPC]
    public void RPC_GameStart()
    {
        GameMode.GameStart();
    }

    [PunRPC]
    private void RPC_GameOver()
    {
        GameStateChange(EGameState.Result);
        EventManager.Instance.GameOver();
    }
    
    public override void OnDisable()
    { 
        base.OnDisable();
        EventManager.Instance.OnLoadFinished -= RequestGameStart;
    }
}



