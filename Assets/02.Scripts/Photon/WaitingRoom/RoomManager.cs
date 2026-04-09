using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonPlayer = Photon.Realtime.Player;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PhotonView))]
public class RoomManager : PhotonSingleton<RoomManager>
{
    private Room _room;
    private PhotonView _photonView;
    public int MaxPlayerCount = 4;
    
    // 스폰, 레디, 초기화, 플레이어 리스트 분리
    public RoomInitializer Initializer; // 초기화
    public RoomReadyCheck ReadyCheck;   // 준비
    public RoomPlayerList PlayerList;  // 현재 플레이어리스트 관리
    public PlayerSpawner Spawner;
    
    public EMap SelectedMap;      // 맵 선택하기
    public EInGameTeam SelectedTeam;
    
    private bool _initialized = false;  // Init 한번만 부르게 하기

    protected override void Awake()
    {
        InputHandler.BlockInput = true;
        base.Awake();

        _photonView = GetComponent<PhotonView>();
        _room = PhotonNetwork.CurrentRoom;
        
        ReadyCheck = new RoomReadyCheck();
        Initializer = new RoomInitializer();
        
        EventManager.Instance.OnPlayerLeft += PlayerLeft;
    }

    // 방 세팅 시작 => Init
    public void Start()
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
        _initialized = true;
        SetRoom();
        Initializer.Init(this);
        
        // 인풋 막기
        InputHandler.BlockInput = true;
        PopupManager.Instance.Open(EPopupType.UI_TempStorage, () => InputHandler.BlockInput = false);
        
        // 인게임 채팅 채널 자동 참가
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.JoinInGameChannel();
        }
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
        
        _room.IsVisible = false;
        _room.IsOpen = false;
        
        PhotonNetwork.DestroyAll(); // 오브젝트들 모두 제거
        
        if (SelectedMap == EMap.Random)
        {
            int max = (int)EMap.Count - 1;
            int index = Random.Range(1, max);
            SelectedMap = (EMap)index;
        }
        
        PhotonNetwork.LoadLevel(SelectedMap.ToString());
    }
    
    //현재 이 방에 있는 플레이어들의 계정 정보
    private void SetRoom()
    {
        InputHandler.BlockInput = false;
        int[] playerList = new int[MaxPlayerCount];
        
        if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(EProperties.PlayerList.ToString()) == false ||
           PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] == null)
        {
            // 만약 방에 처음 들어왔다면 
            PlayerList = new RoomPlayerList(playerList);
            
            if (PhotonNetwork.IsMasterClient)
            {
                PlayerList.AddPlayerPlacement(PhotonNetwork.LocalPlayer);
                EventManager.Instance.RoomDataChanged();
                
                _room.IsVisible = true;
                _room.IsOpen = true;
            }
         
            Initializer.PlayerInitial(1);   
            return;
        }
        
        // 한판 끝나고 돌아왔을 때 플레이어 체크, 원래 있던 플레이어들 refresh
        int[] players = _room.CustomProperties[EProperties.PlayerList.ToString()] as int[];
        PlayerList = new RoomPlayerList(players);
        PlayerList.GetPlayerList(players);
        PlayerList.PlayerListCheck(); // 현재 플레이어와 지금 플레이어의 차이를 체크 
        
        if (PhotonNetwork.IsMasterClient)
        {
            _room.IsVisible = true;
            _room.IsOpen = true;
        }

        EventManager.Instance.RoomDataChanged();
    }
    
    // 커스텀 프로퍼티가 바뀌면 적용되는 이벤트 함수 => 레디를 했는가? 정보창 레디 변경 how? 커스텀 프로퍼티를 이용해서
    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(EProperties.IsReady.ToString())) // 레디 변경
        {
            EventManager.Instance.ReadyChange(targetPlayer);
        }
        
        if (changedProps.ContainsKey(EProperties.Team.ToString()))  // 팀 변경
        {
            if (targetPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                SelectedTeam = (EInGameTeam)changedProps[EProperties.Team.ToString()];
            }
            EventManager.Instance.TeamChanged();
            EventManager.Instance.PlayerColorChanged(targetPlayer.ActorNumber, (EInGameTeam)changedProps[EProperties.Team.ToString()]);
        }
        
        // 아이템(스킨 포함) 변경 콜백: Bomb, Head, Face, Chest, Cape 모두 감지
        if (changedProps.ContainsKey(EItemType.Bomb.ToString()) ||
            changedProps.ContainsKey(EItemType.SubBomb.ToString())||
            changedProps.ContainsKey(EItemType.Head.ToString()) ||
            changedProps.ContainsKey(EItemType.Face.ToString()) ||
            changedProps.ContainsKey(EItemType.Chest.ToString()) ||
            changedProps.ContainsKey(EItemType.Cape.ToString()))
        {
            EventManager.Instance.RoomDataChanged();

            if (targetPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                EventManager.Instance.PlayerItemChanged();
            }
        }
    }

    // 다른 플레이어가 방에 들어왔을 때 위치를 정해준다. => 마스터가 다른 플레이어들에게 RPC를 쏴주는 방식
    // => 다른 플레이어들에게 플레이어 리스트를 전달하고 각자 로컬에서 알아서 UI 리프레시하는 방식
    public override void OnPlayerEnteredRoom(PhotonPlayer newPlayer)
    {
        EventManager.Instance.PlayerItemChanged();
        
        PlayerList.AddPlayerPlacement(newPlayer); // 마스터가 가지고 있는 리스트 업데이트 해주고
        EventManager.Instance.RoomDataChanged();
    }
    
    public void PlayerLeft(PhotonPlayer player)
    {
        PlayerList.SubPlayerPlacement(player);
        EventManager.Instance.RoomDataChanged();
    }
    
    // 맵 변경시 콜백
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(ERoomProperties.MapSelected.ToString()) &&
            propertiesThatChanged[ERoomProperties.MapSelected.ToString()] != null)
        {
            SelectedMap = (EMap)propertiesThatChanged[ERoomProperties.MapSelected.ToString()];
            EventManager.Instance.MapChanged(SelectedMap);
            MapDataManager.Instance.LoadMapData();
        }
        
        if (propertiesThatChanged.ContainsKey(ERoomProperties.GameMode.ToString()))
        {
            Debug.Log("GameModechange " + propertiesThatChanged[ERoomProperties.GameMode.ToString()]);
        }
    }

    // 방장이 바뀌면 콜백
    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        EventManager.Instance.MasterChanged();
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        Hashtable table = new Hashtable()
        {
            {EProperties.IsReady.ToString(), false}
        };
        
        newMasterClient.SetCustomProperties(table);
    }
  
    // 방을 나갈 때 채팅 채널 퇴장
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        Debug.Log("[RoomManager] 방을 나갔습니다. 인게임 채팅 채널 퇴장");

        // 인게임 채팅 채널 퇴장
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.LeaveInGameChannel();
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        EventManager.Instance.OnPlayerLeft -= PlayerLeft;
    }

    private void Update()
    {
        if (InputHandler.GetKeyDown(KeyCode.A))
        {
            PopupManager.Instance.Open(EPopupType.UI_TempStorage);
        }
    }
}
    
    
