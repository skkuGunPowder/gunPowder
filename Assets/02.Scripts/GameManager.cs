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
    [SerializeField]private float _timer;
    
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
            _photonView.RPC(nameof(RPC_GameResultCheck), RpcTarget.All);
        }
    }
    // 게임 종료
    // 프로퍼티가 바뀌었을 때 호출되는 함수
    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer ,Hashtable changedProps)
    {
        if (_currentGameState == EGameState.Waiting)
        {
            return;
        }
        
        if (changedProps.ContainsKey(EProperties.IsDead.ToString()) && changedProps[EProperties.IsDead.ToString()] != null)
        {   
            if (PlayerDeadCheck())
            {
                _photonView.RPC(nameof(RPC_GameResultCheck), RpcTarget.All);
            }
        }
        
    }
    
    private void GameOver()
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
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(ESceneList.Map4.ToString());  
            }
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
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        int playtime = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.PlayTime}"].ToString()) * 60;
        _timer = playtime;
        
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
    
    // 타임 오버가 되었을 때 로컬로 나의 프로퍼티를 보낸다.
    [PunRPC]
    private void RPC_GameResultCheck()
    {
        PhotonPlayer player = PhotonNetwork.LocalPlayer;

        if ((bool)player.CustomProperties[EProperties.IsDead.ToString()])
        {
            return;
        }
        
        GameObject[] playerObject = GameObject.FindGameObjectsWithTag("Player");
        PlayerStat mine = null; 
        foreach (GameObject ob in playerObject)
        {
            if (ob.GetComponent<PhotonView>().IsMine)
            {
                mine = ob.GetComponent<PlayerStat>();
                break;
            }
        }

        if (mine == null)
        {
            throw new Exception("스탯을 찾지 못했습니다.");
        }
        
        Hashtable properties = new Hashtable()
        {
            {EProperties.IsDead.ToString(), true},
            {EProperties.Kill.ToString(), mine.TotalKillCount},
            {EProperties.Damage.ToString(), mine.TotalDamage},
            {EProperties.SurvivorTime.ToString(), SurvivorTime()}

        };
        
        player.SetCustomProperties(properties);
        Debug.Log("타임 오버 : 내 자신 프로퍼티 전달" + $"{player.CustomProperties[EProperties.Kill]}");
        GameOver();
    }
    
    public int SurvivorTime()
    {
        return (int)_timer;
    }
}



