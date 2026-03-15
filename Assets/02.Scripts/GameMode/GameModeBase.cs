using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class GameModeBase : MonoBehaviour
{
    /// <summary>
    /// 게임 모드 모두가 사용할 공통 함수
    /// 1. 게임 종료 : 각 규칙에 따른 종료
    /// 2. 부활 지점 설정, Get
    /// 3. 처음 플레이어 소환
    /// </summary>
    [Header("스폰, 부활")]
    [SerializeField] private Transform _resurrectPoint; 
    
    protected Dictionary<EModeState,GameModeStateBase> _stateDictionary = new Dictionary<EModeState, GameModeStateBase>();
    protected GameModeStateBase _currentState;
    protected EModeState _nextState; // 모든 유저가 준비가 되었을 때 이동
    
    
    public EInGameTeam WinningTeam;
    public GameObject MyPlayer;
    
    protected PlayerSpawner _playerSpawner;
    protected PhotonView _photonView;
    
    private int _playerCount;
    
    /// <summary>
    /// 스폰 포인트 설정하기
    /// </summary>
    protected virtual void Awake()
    {
        if (_playerSpawner == null)
        {
            _playerSpawner = GetComponent<PlayerSpawner>();
        }

        if (_photonView == null)
        {
            _photonView = GetComponent<PhotonView>();
        }
    }
    
    protected virtual void Start()
    {
        
    }
    
    protected virtual void Update()
    {
        _currentState?.Tick();
    }
    
    // 플레이어 소환
    public virtual void SpawnPlayer(int[] playerList)
    {
        for (int i = 0; i < playerList.Length; i++)
        {
            if (playerList[i] != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                continue;
            }
            
            MyPlayer = _playerSpawner.GeneratePlayers(i);
        }
    }

    public virtual void GameStart()
    {
        // 첫 상태 정해주기
    }

    public void CheckState(EModeState state)
    {
        _nextState = state;
        _photonView.RPC(nameof(RPC_CheckState), RpcTarget.MasterClient);
    }
    
    // 현재 방 유저들 모두 스테이트를 변경했는지 체크 >> 방장이 체크 후 RPC 전달
    [PunRPC]
    public void RPC_CheckState()
    {
        _playerCount += 1;

        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        if (_playerCount == PhotonNetwork.PlayerList.Length)
        {
            _photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All);
        }
    }
    
    [PunRPC]
    public void RPC_ChangeState()
    {
        _playerCount = 0;
        ChangeState(_nextState);
    }
    
    // 게임 상태 변경 
    private void ChangeState(EModeState state)
    {
        _currentState?.Exit();
        _currentState = _stateDictionary[state];
        _currentState.Enter();
    }
    
    /// <summary>
    /// 부활 지점이 변경되어야 하는 경우 다른 Transform으로 교체
    /// </summary>
    public void SetResurrectPoint(Transform point)
    {
        _resurrectPoint = point;
    }
    
    /// <summary>
    /// 부활 지점 Get
    /// </summary>
    public Transform GetResurrectPoint()
    {
        return _resurrectPoint;
    }
    
    /// <summary>
    /// 게임 종료 시키기 : 바로 게임 종료 연출이 나옴
    /// 각 게임 규칙에 따라 호출해주면 됨
    /// </summary>
    public virtual void GameOver()
    { 
        // 연출 종료
        GameManager.Instance.RequestGameOver();
    }    
}
