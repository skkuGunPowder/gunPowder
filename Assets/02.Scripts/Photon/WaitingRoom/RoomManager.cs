using System;
using System.Collections.Generic;
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
        base.Awake();

        _photonView = GetComponent<PhotonView>();
        _room = PhotonNetwork.CurrentRoom;
        
        ReadyCheck = new RoomReadyCheck();
        Initializer = new RoomInitializer();
        
        EventManager.Instance.OnPlayerChanged += PlayerLeft;
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
        Initializer.Init(this);
        
        SetRoom();
        EventManager.Instance.TeamChanged();
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

        Initializer.SetPlayerList(PlayerList.PlayerSlotList.ToArray());
        
        _room.IsVisible = false;
        _room.IsOpen = false;
        
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
     
        if (_room.CustomProperties.ContainsKey(EProperties.PlayerList.ToString()) == false)
        {
            // 이 방에 처음 들어온 사람들 초기 세팅
            int[] playerList = new int[MaxPlayerCount];
            PlayerList = new RoomPlayerList(playerList);
            
            if (PhotonNetwork.IsMasterClient)
            {
                PlayerList.AddPlayerPlacement(PhotonNetwork.LocalPlayer);
                EventManager.Instance.RoomDataChanged();
                
                _room.IsVisible = true;
                _room.IsOpen = true;
            }
            return;
        }


        int[] players = _room.CustomProperties[EProperties.PlayerList.ToString()] as int[];
        PlayerList = new RoomPlayerList(players);
        PlayerList.PlayerListCheck(); // 현재 플레이어와 지금 플레이어의 차이를 체크 
        Initializer.SetPlayerList();  // 프로퍼티 지우기

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
            EventManager.Instance.ReadyChange();
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
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        PlayerList.AddPlayerPlacement(newPlayer); // 마스터가 가지고 있는 리스트 업데이트 해주고
        _photonView.RPC(nameof(Rpc_UpdateSlots), RpcTarget.All, PlayerList.PlayerSlotList.ToArray()); // 전달
    }
    
    public void PlayerLeft(PhotonPlayer player)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        PlayerList.SubPlayerPlacement(player);
        _photonView.RPC(nameof(Rpc_UpdateSlots), RpcTarget.All, PlayerList.PlayerSlotList.ToArray());
    }
    
    [PunRPC]
    public void Rpc_UpdateSlots(int[] actorNumbers)
    {
        PlayerList.GetPlayerList(actorNumbers);
        EventManager.Instance.RoomDataChanged();
    }
    
    // 맵 변경시 콜백
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(ERoomProperties.MapSelected.ToString()) && propertiesThatChanged[ERoomProperties.MapSelected.ToString()] != null)
        {
            SelectedMap = (EMap)propertiesThatChanged[ERoomProperties.MapSelected.ToString()];
            EventManager.Instance.MapChanged(SelectedMap);
            MapDataManager.Instance.LoadMapData();
        }

        if (propertiesThatChanged.ContainsKey(ERoomProperties.Life.ToString()) &&
            propertiesThatChanged[ERoomProperties.Life.ToString()] != null)
        {
            EventManager.Instance.RoomDataChanged();
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
  
    public override void OnDisable()
    {
        base.OnDisable();
        EventManager.Instance.OnPlayerChanged -= PlayerLeft;
    }

}
    
    
