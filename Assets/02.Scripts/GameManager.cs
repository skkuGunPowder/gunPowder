using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonPlayer = Photon.Realtime.Player;
using DG.Tweening;
[RequireComponent(typeof(PhotonView))]
public class GameManager : MonoBehaviourPunCallbacks
{
    private PhotonView _photonView;
    private EGameState _currentGameState;
    private float _timer;
    public GameObject GameOverScreen;
    
    private void Awake()
     {
         
         _photonView = GetComponent<PhotonView>();
     }

    private void Start()
    {
        Hashtable load = new Hashtable()
        {
            { EProperties.IsLoad.ToString(), true }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(load);

        _timer = PlayerSettingManager.Instance.PlayTime;

    }

    // 게임 시작
    private void Update()
    {
        if (_currentGameState != EGameState.Playing)
        {
            return;
        }
        
        _timer -= Time.deltaTime;
        
    }
    
    // 게임 종료
    // 프로퍼티가 바뀌었을 때 호출되는 함수
    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer ,Hashtable changedProps)
    {
        Debug.Log("좀 돼라");
        if (changedProps.ContainsKey(EProperties.IsDead.ToString()) && changedProps[EProperties.IsDead.ToString()] != null)
        {
            PlayerDeadCheck();
        }
        if (changedProps.ContainsKey(EProperties.IsLoad.ToString()) && changedProps[EProperties.IsLoad.ToString()] != null)
        {
            GameStart();
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
    
    private void GameStart()
    {
        if (PlayerLoadSceneCheck() == false)
        {
            return;
        }

        Debug.Log($"현재 게임 상태 : {_currentGameState.ToString()}");
        _photonView.RPC(nameof(RPC_RequestGameStart), RpcTarget.All, (int)EGameState.Playing);
    }
    
    // 죽은 사람 체크하기
    private void PlayerDeadCheck()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        List<PhotonPlayer> playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
        foreach (PhotonPlayer p in playerList)
        {
            bool isLoaded = p.CustomProperties.ContainsKey(EProperties.IsLoad.ToString()) && (bool)p.CustomProperties[EProperties.IsLoad.ToString()];
            Debug.Log($"Player {p.NickName} - SceneLoaded: {isLoaded}");
            if (isLoaded == false)
            {
                Debug.Log("아직 준비 안됨");
                return;
            }
        }
        
        _photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
        
    }
    // 모두가 다 들어왔는가?
    private bool PlayerLoadSceneCheck()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return false;
        }
        
        List<PhotonPlayer> playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);

        int dead = 0;
        foreach (PhotonPlayer p in playerList)
        {
            bool isLoaded = p.CustomProperties.ContainsKey(EProperties.IsLoad.ToString()) && (bool)p.CustomProperties[EProperties.IsLoad.ToString()];
            Debug.Log($"Player {p.NickName} - SceneLoaded: {isLoaded}");
            if (isLoaded == false)
            {
                Debug.Log("아직 준비 안됨");
                return false;
            }
            
            dead++;
            Debug.Log($"Player {p.NickName} - SceneLoaded: {dead}");
        }

        if (dead < playerList.Count)
        {
            return false;
        }
        
        return true;
    }

    [PunRPC]
    public void RPC_RequestGameStart(int state)
    {
        _currentGameState = (EGameState)state;
        Debug.Log($"현재 게임 상태 : {_currentGameState.ToString()}");
    }
}



