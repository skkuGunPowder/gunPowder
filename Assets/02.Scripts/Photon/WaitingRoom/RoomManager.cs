using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonPlayer = Photon.Realtime.Player;

[RequireComponent(typeof(PhotonView))]
public class RoomManager : PhotonSingleton<RoomManager>
{
    private Room _room;
    public PlayerSpawner Spawner;
    //리스트로 정보칸 들어가게 하기 => 플레이어 칸 정하기
    private List<int> _playerSlotList;
    public List<int> PlayerSlotList => _playerSlotList;

    public ESceneList SelectedMap;      // 맵 선택하기
    public EInGameTeam SelectedTeam;
    
    private bool _initialized = false;  // Init 한번만 부르게 하기

    private PhotonView _photonView;
    protected override void Awake()
    {
        base.Awake();

        _photonView = GetComponent<PhotonView>();
        _room = PhotonNetwork.CurrentRoom;
        
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
        SetProperties();
        SetRoom();

        _initialized = true;
        GeneratePlayer();
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
            { EProperties.IsLoad.ToString(), false}
        };
        
        if (PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()] == null)
        {
            PhotonPlayer[] players = PhotonNetwork.PlayerList;
            HashSet<EInGameTeam> usedTeams = new HashSet<EInGameTeam>();

            foreach (PhotonPlayer player in players)
            {
                if (player.CustomProperties.TryGetValue(EProperties.Team.ToString(), out object teamObj))
                {
                    EInGameTeam team = (EInGameTeam)teamObj;
                    usedTeams.Add(team);
                }
            }

            // 가능한 팀 중에서 사용되지 않은 팀 찾기
            EInGameTeam myTeam = EInGameTeam.Red; // 기본값
            foreach (EInGameTeam team in Enum.GetValues(typeof(EInGameTeam)))
            {
                if (!usedTeams.Contains(team))
                {
                    myTeam = team;
                    break;
                }
            }
            
            ready.Add(EProperties.Team.ToString(), (int)myTeam);
        }
        else
        {
            SelectedTeam = (EInGameTeam)PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()];
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
                0, 0, 0, 0
            };

            if (PhotonNetwork.IsMasterClient)
            {
                PlayerPlacement(PhotonNetwork.LocalPlayer);
                EventManager.Instance.RoomDataChanged();
            }

            return;
        }


        int[] players = _room.CustomProperties[EProperties.PlayerList.ToString()] as int[];
        _playerSlotList = new List<int>(players);

        PlayerListCheck();
        
        _room.IsVisible = true;
        
        EventManager.Instance.RoomDataChanged();
    }
    
    // 현재 받은 플레이어 리스트와 지금 있는 사람들의 리스트를 비교해서 플레이어 리스트 정리
    private void PlayerListCheck()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        
        foreach (PhotonPlayer player in players)
        {
            if (_playerSlotList.Contains(player.ActorNumber))
            {
                return;
            }

            for (int i = 0; i < _playerSlotList.Count; i++)
            {
                if (_playerSlotList[i] == player.ActorNumber)
                {
                    _playerSlotList[i] = 0;
                    break;   
                }
            }
        }
    }
    // 커스텀 프로퍼티가 바뀌면 적용되는 이벤트 함수 => 레디를 했는가? 정보창 레디 변경 how? 커스텀 프로퍼티를 이용해서
    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey($"{EProperties.IsReady}"))
        {
            EventManager.Instance.ReadyChange();
        }
        
        if (changedProps.ContainsKey($"{EProperties.Team}"))
        {
            if (targetPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                SelectedTeam = (EInGameTeam)changedProps[$"{EProperties.Team}"];
            }
            EventManager.Instance.TeamChanged();
            EventManager.Instance.PlayerColorChanged(targetPlayer.ActorNumber, (EInGameTeam)changedProps[$"{EProperties.Team}"]);
        }
        if (changedProps.ContainsKey($"{EItemType.Bomb}"))
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
        
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerPlacement(newPlayer); // 마스터가 가지고 있는 리스트 업데이트 해주고
            _photonView.RPC(nameof(Rpc_OnEnterUpdateSlots), RpcTarget.All, _playerSlotList.ToArray()); // 전달
        }
    }
    public void PlayerLeft(PhotonPlayer player)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        int num = player.ActorNumber;
        
        for (int i = 0; i < _playerSlotList.Count; i++)
        {
            if (_playerSlotList[i] == num)
            {
                _playerSlotList[i] = 0;
                break;
            }
        }
        _photonView.RPC(nameof(Rpc_OnLeftUpdateSlots), RpcTarget.All, _playerSlotList.ToArray());
        
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
    public void Rpc_OnLeftUpdateSlots(int[] actorNumbers)
    {
        _playerSlotList = new List<int>(actorNumbers);
        EventManager.Instance.RoomDataChanged();
    }
    [PunRPC]
    public void Rpc_OnEnterUpdateSlots(int[] actorNumbers)
    {
        _playerSlotList = new List<int>(actorNumbers);
        EventManager.Instance.RoomDataChanged();
    }
    // 맵 변경시 콜백
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(EProperties.MapSelected.ToString()) && propertiesThatChanged[EProperties.MapSelected.ToString()] != null)
        {
            SelectedMap = (ESceneList)propertiesThatChanged[$"{EProperties.MapSelected}"];
            EventManager.Instance.MapChanged();
        }

        if (propertiesThatChanged.ContainsKey(EProperties.Life.ToString()) &&
            propertiesThatChanged[EProperties.Life.ToString()] != null)
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
    
    
