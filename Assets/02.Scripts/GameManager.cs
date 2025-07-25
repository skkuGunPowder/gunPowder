using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonPlayer = Photon.Realtime.Player;
using DG.Tweening;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(LoadSceneChecker))]
public class GameManager : PhotonSingleton<GameManager>
{
    private PhotonView _photonView;
    private EGameState _currentGameState;
    [SerializeField] private float _timer;
    private LoadSceneChecker _loadChecker;
    public GameObject GameOverScreen;

    public List<Transform> FallDeadStartPointList;     // 좌 : 0, 우 : 1
    public List<Transform> FallDeadPathList;           // 좌 : 0, 우 : 1
    public Transform ResurrectPoint;                   // 부활 지점
    
    protected override void Awake()
     {
         base.Awake();

         _photonView = GetComponent<PhotonView>();
         _loadChecker = GetComponent<LoadSceneChecker>();

         _loadChecker.OnLoadFinished += GameStart;
     }

    private void Start()
    {
        _timer = PlayerSettingManager.Instance.PlayTime;
    }

    // 게임 시작
    private void Update()
    {
        GameTimer(); 
    }

    private void GameTimer()
    {
        if (_currentGameState != EGameState.Playing)
        {
            return;
        }
        
        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            _currentGameState = EGameState.GameOver;
            
        }
        
        _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
    }
    // 게임 종료
    // 프로퍼티가 바뀌었을 때 호출되는 함수
    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer ,Hashtable changedProps)
    {
        Debug.Log("플레이어 프로퍼티 변경");
        if (changedProps.ContainsKey(EProperties.IsDead.ToString()) && changedProps[EProperties.IsDead.ToString()] != null)
        {
            if (PlayerDeadCheck())
            {
                _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
            }
        }
    }

    [PunRPC]
    private void RPC_GameOver()
    {
        _currentGameState = EGameState.GameOver;
        
        if (_currentGameState != EGameState.GameOver)
        {
            return;
        }
        
        Sequence gameOverSequence = DOTween.Sequence();
        gameOverSequence.Append(GameOverScreen.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce));
        gameOverSequence.Append(GameOverScreen.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce));
        gameOverSequence.OnComplete(() =>
        {
            PhotonNetwork.LoadLevel(ESceneList.Map4.ToString());
        });
    }
    
    // 캐릭터들 사망 체크하기 = 방장만
    private bool PlayerDeadCheck()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return false;
        }
        
        List<PhotonPlayer> playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);

        int dead = 0;
        
        foreach (PhotonPlayer p in playerList)
        {
            bool isDead = p.CustomProperties.ContainsKey(EProperties.IsDead.ToString()) && (bool)p.CustomProperties[EProperties.IsDead.ToString()];
            Debug.Log($"Player {p.NickName} - Dead: {isDead}");
            if (isDead == false)
            {
                Debug.Log("아직 준비 안됨");
                continue;
            }
            
            dead++;
            
        }

        Debug.Log("현재 죽은 인원 " + dead + "명");
        
        if (dead < playerList.Count)
        {
            return false;
        }
        
        return true;
    }
        
    private void GameStart()
    {
        _photonView.RPC(nameof(RPC_RequestGameStart), RpcTarget.All, (int)EGameState.Playing);
    }

    [PunRPC]
    public void RPC_RequestGameStart(int state)
    {
        _currentGameState = (EGameState)state;
        Debug.Log($"현재 게임 상태 : {_currentGameState.ToString()}");
    }
    
    
}



