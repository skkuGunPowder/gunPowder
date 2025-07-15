using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager Instance;
    
    private List<RoomInfo> _roomInfoList = new List<RoomInfo>();
    public List<RoomInfo> RoomInfoList => _roomInfoList;

    public event Action OnDataChanged;
    
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
    }
    // 방에 보내기
    public void MakeRoom(string roomName, int maxPlayers)
    {
        RoomOptions roomOptions = new RoomOptions();
        
        roomOptions.MaxPlayers = maxPlayers;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
        OnDataChanged?.Invoke();
    }
    // 모든 룸 정보들을 가지고 있어야함 => 방 refresh담당
    // 계정 정보들 가져오기?
    // 
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (PhotonNetwork.InLobby == false)
        {
            return;
        }
        _roomInfoList = roomList;
        OnDataChanged?.Invoke();
        
        Debug.Log(_roomInfoList.Count);
    }
}
