using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

[RequireComponent(typeof(PhotonView))]
public class RoomManager : PhotonSingleton<RoomManager>
{
    private Room _room;
    public PlayerSpawner Spawner;
    //리스트로 정보칸 들어가게 하기 => 플레이어 칸 정하기
    private List<int> _playerSlotList;
    public List<int> PlayerSlotList => _playerSlotList;
    private LoadSceneChecker _loadChecker;
    
    public ESceneList SelectedMap;      // 맵 선택하기
    
    private bool _initialized = false;  // Init 한번만 부르게 하기
    
    private PhotonView _photonView;
    
    protected override void Awake()
    {
        base.Awake();
        
        _photonView = GetComponent<PhotonView>();
        _loadChecker = GetComponent<LoadSceneChecker>();

        // _loadChecker.OnLoadFinished += Init;
        _room = PhotonNetwork.CurrentRoom;


    }
    
    // 방 세팅 시작 => Init
    private void Start()
    {
        if (PhotonNetwork.InRoom == false)
        {
            return;
        }
        
        if (_initialized)
        {
            return;
        }
        
        Init();
    } 
    
    // 처음 방에 들어왔을 때 => 세팅 Init
    public override void OnJoinedRoom()
    {
        if (_initialized)
        {
            return;
        }
        
        Init();
    }
    
    // 방에 들어왔을 때 첫 세팅 하기
    private void Init()
    {
        SetRoom();
        
        _initialized = true;
        GeneratePlayer();
        SetProperties();
        SetCurrentMap();
        
    }

    private void GeneratePlayer()
    {
        Spawner.GeneratePlayers(0);
    }
    // 플레이어가 레디를 했는지 체크했는지 알아보는 커스텀 프로퍼티
    private void SetProperties()
    {
        Hashtable ready = new Hashtable
        {
            { EProperties.IsReady.ToString(), false },
            { EProperties.IsDead.ToString(), false },
        };

        if (PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team] == null)
        {
            ready.Add(EProperties.Team.ToString(), (int)EInGameTeam.Red);
        }
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(ready);
    }
    // 현재 방의 맵이 무엇인가?
    private void SetCurrentMap()
    {
        SelectedMap = (ESceneList)PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.MapSelected}"];
        EventManager.Instance.MapChanged();
    }
    // 준비가 다 되었다면 마스터가 정한 맵으로 이동시킴
    // 확인이 필요한 것 : 1. 방장인가?
    //                  2. 모두 준비가 되었는 가? 
    public void GameStart()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        if (IsPlayerReady() == false)
        {
            return;
        }
        
        Hashtable playerList = new Hashtable()
        {
            {EProperties.PlayerList.ToString(), _playerSlotList.ToArray()}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(playerList);
        
        _room.IsVisible = false;
        PhotonNetwork.LoadLevel(SelectedMap.ToString());
        
    }

    // 사람들이 모두 눌렀는가?
    public bool IsPlayerReady()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;

        foreach (PhotonPlayer player in players)
        {
            if (player.IsMasterClient)
            {
                continue;
            }

            if (player.CustomProperties.ContainsKey($"{EProperties.IsReady}") == false || (bool)player.CustomProperties[$"{EProperties.IsReady}"] == false)
            {
                return false;
            }
        }

        return true;
    }
    //현재 이 방에 있는 플레이어들의 계정 정보
    private void SetRoom()
    { 
        if (_room.CustomProperties.ContainsKey(EProperties.PlayerList.ToString()) == false)
        {
            _playerSlotList = new List<int>()
            {
                0,0,0,0
            };
         
            if (PhotonNetwork.IsMasterClient)
            {
                PlayerPlacement(PhotonNetwork.LocalPlayer);
                EventManager.Instance.RoomDataChanged();
            };
            
            return;
        }
        
        
        int[] players = _room.CustomProperties[EProperties.PlayerList.ToString()] as int[];
        _playerSlotList = new List<int>(players);
        _room.IsVisible = true;
        
        if (PhotonNetwork.IsMasterClient)
        {
            _photonView.RPC(nameof(UpdateSlots),RpcTarget.All, _playerSlotList.ToArray());
        }
    }
    // 커스텀 프로퍼티가 바뀌면 적용되는 이벤트 함수 => 레디를 했는가? 정보창 레디 변경 how? 커스텀 프로퍼티를 이용해서
    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer,Hashtable changedProps)
    {
        if (changedProps.ContainsKey($"{EProperties.IsReady}"))
        {
            EventManager.Instance.ReadyChange();
        }
    }
    
    // 다른 플레이어가 방에 들어왔을 때 위치를 정해준다. => 마스터가 다른 플레이어들에게 RPC를 쏴주는 방식
    // => 다른 플레이어들에게 플레이어 리스트를 전달하고 각자 로컬에서 알아서 UI 리프레시하는 방식
    public override void OnPlayerEnteredRoom(PhotonPlayer newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerPlacement(newPlayer); // 마스터가 가지고 있는 리스트 업데이트 해주고
            _photonView.RPC(nameof(UpdateSlots), RpcTarget.All, _playerSlotList.ToArray()); // 전달
        }
        
    }

    public override void OnPlayerLeftRoom(PhotonPlayer otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerLeft(otherPlayer);
            _photonView.RPC(nameof(UpdateSlots), RpcTarget.All, _playerSlotList.ToArray());
        }
        
    }

    public void PlayerLeft(PhotonPlayer player)
    {
        int num = player.ActorNumber;

        for (int i = 0; i < _playerSlotList.Count; i++)
        {
            if (_playerSlotList[i] == num)
            {
                _playerSlotList[i] = 0;
                break;
            };
        }
    }
    public void PlayerPlacement(PhotonPlayer player)
    {
        for (int i = 0; i < _playerSlotList.Count; i++)
        {
            if (_playerSlotList[i] == 0)
            {
                _playerSlotList[i] = player.ActorNumber;
                break;
            }
        }
        
    }

    [PunRPC]
    public void UpdateSlots(int[] actorNumbers)
    {
        _playerSlotList = new List<int>(actorNumbers);
        EventManager.Instance.RoomDataChanged();
    }

    // 맵 변경시 콜백
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey($"{EProperties.MapSelected}") && propertiesThatChanged[$"{EProperties.MapSelected}"] != null)
        { 
            SelectedMap = (ESceneList)propertiesThatChanged[$"{EProperties.MapSelected}"];
            EventManager.Instance.MapChanged();
        }
    }
    
    // 방장이 바뀌면 콜백
    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        EventManager.Instance.MasterChanged();
    }
    
}
    
    
