using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonPlayer = Photon.Realtime.Player;
using DG.Tweening;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class GameManager : PhotonSingleton<GameManager>
{
    private PhotonView _photonView;
    [SerializeField] private EGameState _currentGameState;
    public EGameState CurrentGameState => _currentGameState;
    [SerializeField]private float _timer;
    public float Timer => _timer;
    private float _InitTime = 0;
    
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

         EventManager.Instance.OnLoadFinished += Init;
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

        _timer -= Time.deltaTime;

        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        if (_timer <= 0)
        {
            _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
        }
    }
    // 게임 종료
    // 프로퍼티가 바뀌었을 때 호출되는 함수
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
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        if ((bool)changedProps[EProperties.IsDead.ToString()])
        {
            int playtime = (int)Mathf.Abs(_timer - _InitTime);
            Hashtable hash = new Hashtable() 
            {
                {EProperties.SurvivorTime.ToString(), playtime} 
            };
            
            targetPlayer.SetCustomProperties(hash);
            Debug.Log($"{targetPlayer.ActorNumber}의 죽은 시간 : {playtime}");
        }   
        Debug.Log(targetPlayer.ActorNumber + "현재 죽은 사람 테스트");
        if (PlayerDeadCheck())
        { 
            _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
        }

        
    }
    
    [PunRPC]
    private void RPC_GameOver()
    {
     
        Sequence gameOverSequence = DOTween.Sequence();
        gameOverSequence.Append(GameOverScreen.transform.DOScale(1f, 3f).SetEase(Ease.OutBounce));
        gameOverSequence.AppendCallback(() =>
        {
            GameResultCheck();
        });
        gameOverSequence.Append(GameOverScreen.transform.DOScale(1f, 3f).SetEase(Ease.OutBounce));
        gameOverSequence.OnComplete(() =>
        {
            
            _currentGameState = EGameState.GameOver;
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(ESceneList.Map4.ToString());  
            }
        });
    }
    
    // 캐릭터들 사망 체크하기 = 방장만
    private bool PlayerDeadCheck()
    {
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

    private void Init()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        _photonView.RPC(nameof(RPC_RequestGameStart), RpcTarget.All, (int)EGameState.Playing);
    }

    [PunRPC]
    public void RPC_RequestGameStart(int state)
    {
        _currentGameState = (EGameState)state;
        SceneManager.UnloadSceneAsync(ESceneList.StartSequence.ToString());;
        OnProfileInit?.Invoke();

        int playtime = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.PlayTime}"].ToString()) * 60;
        
        _InitTime = playtime;

        _timer = _InitTime;
        Debug.Log($"플레이 타임 : {playtime}");
        
        EventManager.Instance.OnLoadFinished -= Init;
    }
    
    // 타임 오버가 되었을 때 로컬로 나의 프로퍼티를 보낸다.
    private void GameResultCheck()
    {
        PhotonPlayer player = PhotonNetwork.LocalPlayer;

        if ((bool)player.CustomProperties[EProperties.IsDead.ToString()])
        {
            return;
        }

        int playTime = (int)Mathf.Abs(_timer - _InitTime);
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        PlayerStat stat = playerObject.GetComponent<PlayerStat>();
        Hashtable properties = new Hashtable()
        {
            {EProperties.IsDead.ToString(), true},
            {EProperties.Kill.ToString(), stat.TotalKillCount},
            {EProperties.Damage.ToString(), stat.TotalDamage}

        };
        if (PhotonNetwork.IsMasterClient)
        {
            properties = new Hashtable()
            {
                {EProperties.IsDead.ToString(), true},
                {EProperties.Kill.ToString(), stat.TotalKillCount},
                {EProperties.Damage.ToString(), stat.TotalDamage},
                {EProperties.SurvivorTime.ToString(), playTime}
            };
        }
        player.SetCustomProperties(properties);
        Debug.Log($"타임 오버 : 내 자신{player.ActorNumber} 프로퍼티 전달" + $"{player.CustomProperties[EProperties.Kill]}");
    }

    
}



