using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
public class VollyballMode : GameModeBase
{
    /// <summary>
    /// 팀에 따라 다른 위치에서 시작해야함.
    /// 한 라운드가 끝나면 초기화?
    /// 2팀으로 운영함
    /// 1. 5점 달성 시 게임 종료
    /// 2. 득점 시 점수 연출 -> 원래 자리로 이동
    /// 3. 플레이어 소환
    /// </summary>
    [Header("부활 위치")]
    [SerializeField] private Transform _redResurrectPoint;
    [SerializeField] private Transform _blueResurrectPoint;
    
    [Header("배구공")]
    [SerializeField] private GameObject _vollyballPrefab;
    [SerializeField] private Transform _vollyballSpawnPoint;
    
    [Header("설정")]
    [SerializeField] private int _maxRound = 5;
    private Dictionary<EInGameTeam, int> _teamScore = new Dictionary<EInGameTeam, int>();
    private EInGameTeam _myTeam;
    private bool _isEnd = false;    
    protected override void Awake()
    {
        base.Awake();
        
        // 플레이어 팀에 따라 부활 위치 설정
        _myTeam = (EInGameTeam)PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()];
        TeamSetting(_myTeam);
        
        // 스코어 초기화
        _teamScore.Clear();
        _teamScore.Add(EInGameTeam.Red,0);
        _teamScore.Add(EInGameTeam.Blue,0);
        
        SubScribe();
    }
    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            RequestScoreGoal(EInGameTeam.Red);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            RequestScoreGoal(EInGameTeam.Blue);
        }
}
    // 팀에 따라 필요한 세팅
    private void TeamSetting(EInGameTeam team)
    {
        switch (team)
        {
            case EInGameTeam.Red : SetResurrectPoint(_redResurrectPoint);
                break;
            case EInGameTeam.Blue : SetResurrectPoint(_blueResurrectPoint);
                break;
            // 나중에 수정 (Green, Yellow가 들어온 경우 대비)
            case EInGameTeam.Green : SetResurrectPoint(_redResurrectPoint);
                break;
            case EInGameTeam.Yellow : SetResurrectPoint(_blueResurrectPoint);
                break;
            default :
                break;
        }
    }

    protected override void Start()
    {
        //팀 스폰 위치에 소환하기
        _playerSpawner.GeneratePlayers((int)_myTeam);
        InstantiateVolleyball();
    }
    
    // 배구공 소환
    private void InstantiateVolleyball()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
     
        if (_isEnd)
        {
            // 5점이 넘었으면 방장이 게임 종료
            GameOver();
            return;
        }
        
        // 방장이 소환
        PhotonNetwork.Instantiate(_vollyballPrefab.name, _vollyballSpawnPoint.position, Quaternion.identity);
    }

    //득점 점수 계산하기 : 플레이어 넘버를 가지고 팀 찾은 후, 그 팀에 점수 추가
    private void RequestScoreGoal(EInGameTeam team)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        // RPC 보내기 위해 int 변환
        _photonView.RPC(nameof(Rpc_ScoreGoal), RpcTarget.All, (int)team);
    }

    // 게임 종료 체크 : 5점 이상이라면 게임 종료 On
    private void EndCheck(EInGameTeam team)
    {
        if (_teamScore[team] >= _maxRound)
        {
            _isEnd = true;
        }
    }
    
    /// <summary>
    /// RPCMethod
    /// </summary>
    [PunRPC]
    private void Rpc_ScoreGoal(int team)
    {
        EInGameTeam inGameTeam = (EInGameTeam)team;
        _teamScore[inGameTeam]++;
        
        if (PhotonNetwork.IsMasterClient)
        {
            EndCheck(inGameTeam);
        }
        
        EventManager.Instance.ScoreUpdate(inGameTeam, _teamScore[inGameTeam]);
    }
    
    private void OnDestroy()
    {
        UnSubScribe();
    }
    
    private void SubScribe()
    {
        EventManager.Instance.OnScoreGoal += RequestScoreGoal;
        EventManager.Instance.OnGameRespawn += InstantiateVolleyball;
    }
    
    private void UnSubScribe()
    {
        EventManager.Instance.OnScoreGoal -= RequestScoreGoal;
        EventManager.Instance.OnGameRespawn -= InstantiateVolleyball;
    }
    
}
