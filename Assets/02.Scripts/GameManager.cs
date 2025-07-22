using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonPlayer = Photon.Realtime.Player;

[RequireComponent(typeof(PhotonView))]
public class GameManager : MonoBehaviourPunCallbacks
{
    private PhotonView _photonView;
    private EGameState _currentGameState;
    private float _timer;
    private bool _test = false;
    private void Awake()
     {
         
         _photonView = GetComponent<PhotonView>();
     }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        
        _timer = PlayerSettingManager.Instance.PlayTime;

    }
    // 필요한거 
    // 타이머
    
    // 
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
            
        }
        if (changedProps.ContainsKey(EProperties.IsLoad.ToString()) && changedProps[EProperties.IsLoad.ToString()] != null)
        {
            GameStart();
        }
        
    }

    public void OnClickChanged()
    {
        Hashtable load = new Hashtable()
        {
            { EProperties.IsLoad.ToString() , !_test },
        };
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(load);
        
        _test = !_test;
        Debug.Log($"{load[EProperties.IsLoad.ToString()]}");
        Debug.Log("bool");
    }
    private void GameStart()
    {
        if (PlayerLoadSceneCheck() == false)
        {
            return;
        }

        Debug.Log($"현재 게임 상태 : {_currentGameState.ToString()}");
        _photonView.RPC(nameof(RPC_RequestGameStart), RpcTarget.All, EGameState.Playing);
    }
    // 모두가 다 들어왔는가?
    private bool PlayerLoadSceneCheck()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return false;
        }
        
        List<PhotonPlayer> playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
        foreach (PhotonPlayer p in playerList)
        {
            bool isLoaded = p.CustomProperties.ContainsKey(EProperties.IsLoad.ToString()) && (bool)p.CustomProperties[EProperties.IsLoad.ToString()];
            Debug.Log($"Player {p.NickName} - SceneLoaded: {isLoaded}");
            if (isLoaded == false)
            {
                Debug.Log("아직 준비 안됨");
                return false;
            }
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
