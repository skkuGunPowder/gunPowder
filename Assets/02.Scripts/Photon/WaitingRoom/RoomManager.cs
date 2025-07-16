using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    private Room _room;
    
    public int MaxPlayers => _room.MaxPlayers; // 이 방에 최대 인원
    public int CurrentPlayer; // 이 방에 현재 인원
    
    //리스트로 정보칸 들어가게 하기
    private List<int> _playerSlotList;
    public List<int> PlayerSlotList => _playerSlotList;
    
    public event Action OnDataChanged; 
    
    // 우리는 어떤 맵으로 가야하는 가?
    public ESceneList SelectedMap;
    private bool _initialized = false;
    
    private PhotonView _photonView;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        _photonView = GetComponent<PhotonView>();
    }

    // 플레이어가 체크했는지 알아보는 커스텀 프로퍼티
    private void SetReady()
    {
        Hashtable ready = new Hashtable { { "isReady", false } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(ready);
        Debug.Log("커스텀 프로퍼티가 적용이 되었습니다."  + ready["isReady"]);
    }
    
    // 방 세팅 시작
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
        
        Debug.Log("Start");
        Init();
    }

    private void Init()
    {
        _initialized = true;
        SetRoom();
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerPlacement(PhotonNetwork.LocalPlayer);
        }
        SetReady();
        
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

        PhotonNetwork.LoadLevel(SelectedMap.ToString());
    }

    // 사람들이 모두 눌렀는가?
    public bool IsPlayerReady()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;

        foreach (PhotonPlayer player in players)
        {
            Debug.Log(player.NickName + $"{player.CustomProperties.ContainsKey("isReady")}");
            if (player.IsMasterClient)
            {
                continue;
            }

            if (player.CustomProperties.ContainsKey("isReady") == false || (bool)player.CustomProperties["isReady"] == false)
            {
                return false;
            }
        }

        return true;
    }
    // 현재 이 방의 최고 인원은 몇인가?

    //현재 이 방에 있는 플레이어들의 계정 정보
    private void SetRoom()
    {
        _room = PhotonNetwork.CurrentRoom;
        _playerSlotList = new List<int>()
        {
            0,0,0,0
        };
        
    }
    
    public override void OnJoinedRoom()
    {
        if (_initialized)
        {
            return;
        }
        
        Debug.Log("OnJoinedRoom");
        Init();
       
    }

    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer,ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("isReady"))
        {
            OnDataChanged?.Invoke();
        }
    
    }
    
    public override void OnPlayerEnteredRoom(PhotonPlayer newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("OnPlayerEnteredRoom");
            PlayerPlacement(newPlayer);
            _photonView.RPC(nameof(UpdateSlots), RpcTarget.All, _playerSlotList.ToArray());
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
        foreach (int actorNumber in _playerSlotList)
        {
            Debug.Log($"{actorNumber}");
        }
        OnDataChanged?.Invoke();
    }
}
    
    
