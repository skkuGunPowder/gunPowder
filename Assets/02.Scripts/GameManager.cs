using System;
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
    [SerializeField] private EGameState _currentGameState;
    public EGameState CurrentGameState => _currentGameState;
    private float _timer;
    
    private Dictionary<int, int> _playTimeDictionary = new Dictionary<int, int>();
    private Dictionary<int, int> _playerRankDictionary = new Dictionary<int, int>();
    
    private LoadSceneChecker _loadChecker;
    public GameObject GameOverScreen;
    
    public List<Transform> FallDeadStartPointList;     // 좌 : 0, 우 : 1
    public List<Transform> FallDeadPathList;           // 좌 : 0, 우 : 1
    public Transform ResurrectPoint;                   // 부활 지점
    
    public event Action OnProfileInit;
    protected override void Awake()
     {
         base.Awake();

         _photonView = GetComponent<PhotonView>();
         _loadChecker = GetComponent<LoadSceneChecker>();
         
         if (_currentGameState == EGameState.Waiting)
         {
             return;
         }
         _loadChecker.OnLoadFinished += GameStart;
     }
    
    // 게임 시작
    private void Update()
    {
        if (_currentGameState != EGameState.Playing)
        {
            return;
        }
        GameTimer(); 
    }

    private void GameTimer()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        _timer -= Time.deltaTime;
        
        if (_timer <= 0)
        {
            _currentGameState = EGameState.GameOver;
            _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
            GameResultCheck();
        }
    }
    // 게임 종료
    // 프로퍼티가 바뀌었을 때 호출되는 함수
    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer ,Hashtable changedProps)
    {
        if (changedProps.ContainsKey(EProperties.IsDead.ToString()) && changedProps[EProperties.IsDead.ToString()] != null)
        {   
            if (PlayerDeadCheck())
            {
                _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
                GameResultCheck();
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
        Debug.Log(playerList.Count);
        
        
        int dead = 1;
        
        foreach (PhotonPlayer p in playerList)
        {
            bool isDead = p.CustomProperties.ContainsKey(EProperties.IsDead.ToString()) && (bool)p.CustomProperties[EProperties.IsDead.ToString()];
            Debug.Log($"Player {p.NickName}{p.ActorNumber} - Dead: {isDead}");
            if (isDead == false)
            {
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
        Debug.Log("게임 시작");
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        int playtime = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.PlayTime}"].ToString()) * 60;
        _timer = playtime;
        Debug.Log($"게임 시작 : 게임 시간 : {_timer}");
        
        _photonView.RPC(nameof(RPC_RequestGameStart), RpcTarget.All, (int)EGameState.Playing);
    }

    [PunRPC]
    public void RPC_RequestGameStart(int state)
    {
        _currentGameState = (EGameState)state;
        OnProfileInit?.Invoke();
        if (_currentGameState == EGameState.Waiting)
        {
            return;
        }
        _loadChecker.OnLoadFinished -= GameStart;
    }
    
    // 등수 체크하는 방법
    private void RankCheck()
    {
        // 이미 죽은 상태면 넘어가고
        // 살아있는 상태로 끝났다면 점수를 체크해서 스택에 넣어줌
        
    }
    
    // 타이머가 0이 되었을 때 or 게임이 끝났을 때
    private void GameResultCheck()
    {
        if (PhotonNetwork.IsMasterClient == false && _currentGameState != EGameState.GameOver)
        {
            return;
        }
        RankCheck();
        
    }
    
}



