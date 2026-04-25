using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(PhotonView))]
public class GameModeBase : MonoBehaviourPunCallbacks
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

    public bool IsFirstSpawn { get; private set; } = true; // 첫 번째 스폰 여부
    public GameObject MyPlayer;
    
    protected PlayerSpawner _playerSpawner;
    public PlayerSpawner PlayerSpawner => _playerSpawner;
    protected PhotonView _photonView;
    
    private int _playerCount;
    
    // 동일 룸 프로퍼티 재설정 시 이벤트 누락 방지용 카운터
    private int _stateChangeId = 0;
    
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
        EventManager.Instance.OnFindPlayer += SetMyPlayer;
    }
    
    protected virtual void Update()
    {
        _currentState?.Tick();
    }
    
    
    public void SetFirstSpawnComplete()
    {
        IsFirstSpawn = false;
    }

    private void SetMyPlayer(GameObject player)
    {
        MyPlayer = player;
    }
    /// <summary>
    /// 프로퍼티가 바뀌었을 때 호출되는 함수
    /// 플레이어가 죽을 때마다 죽은 플레이어들 체크하기
    /// </summary>
    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer ,Hashtable changedProps)
    {
        if (GameManager.Instance.CurrentGameState == EGameState.Waiting || GameManager.Instance.CurrentGameState == EGameState.GameOver)
        {
            return;
        }
        
        if (changedProps.ContainsKey(EItemType.Bomb.ToString()) || changedProps.ContainsKey(EItemType.SubBomb.ToString()))
        {
            EventManager.Instance.ReadyChange(targetPlayer);
            
            // 처음 폭탄 선택에서 발생 예정
            if (targetPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                EventManager.Instance.PlayerItemChanged();
            }
        }

        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        // 죽음 처리
        if (changedProps.ContainsKey(EProperties.DeadCheck.ToString()) || changedProps[EProperties.DeadCheck.ToString()] != null)
        {
            if ((bool)changedProps[EProperties.DeadCheck.ToString()])
            {
                EventManager.Instance.TimeCheck(targetPlayer);
            }
        }
        
        if (changedProps.ContainsKey(EProperties.IsDead.ToString()) || changedProps[EProperties.IsDead.ToString()] != null)
        {
            if ((bool)changedProps[EProperties.IsDead.ToString()])
            {
                EventManager.Instance.GameStateChangeCheck(targetPlayer);
            }
        }
        
    }

    // 게임의 상태가 변경될때 룸 프로퍼티로 전달
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {        
        // 상태 동기화 처리
        if (!propertiesThatChanged.ContainsKey(ERoomProperties.StateChange.ToString()))
        {
            return;
        }
        
        int state = (int)propertiesThatChanged[ERoomProperties.StateChange.ToString()];
        EModeState modeState = (EModeState)state;
        CheckChangeComplete(modeState);
        
    }

    public virtual void GameStart()
    {
        // 첫 상태 정해주기
    }

    public void RequestStateChange(EModeState state)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        
        if (state == EModeState.None)
        {
            return;
        }

        int stateInt = (int)state;
        _stateChangeId++;
        
        Hashtable properties = new Hashtable
        {
            { ERoomProperties.StateChange.ToString(), stateInt },
            { ERoomProperties.StateChangeId.ToString(), _stateChangeId }    // 룸프로퍼티 업데이트 콜백 함수가 언제나 호출될 수 있도록 
        };
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void CheckChangeComplete(EModeState state)
    {
        _nextState = state;
        _photonView.RPC(nameof(RPC_CheckState), RpcTarget.MasterClient);
    }
    
    [PunRPC]
    public void RPC_CheckState(PhotonMessageInfo info)
    {
        CheckState();
    }
    
    
    public void CheckState()
    {
        _playerCount += 1;

        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        if (_playerCount == PhotonNetwork.PlayerList.Length)
        {
            _playerCount = 0;
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
        EventManager.Instance.OnFindPlayer -= SetMyPlayer;
        GameManager.Instance.RequestGameOver();
    }    
}
