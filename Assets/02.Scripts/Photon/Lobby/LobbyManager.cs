using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class LobbyManager : PhotonSingleton<LobbyManager>
{
    private List<RoomInfo> _roomInfoList = new List<RoomInfo>();
    public List<RoomInfo> RoomInfoList => _roomInfoList;
    
    [Header("처음 맵 설정")] public EMap InitialMap = EMap.Forest1;
    
    protected override void Awake()
    {
        base.Awake();
        ClientManager.PlayBGM("Lobby");
    }
    
    // 방에 보내기
    public void MakeRoom(string roomName, int maxPlayers, int playTime, int life, int gunpowder, int decline, bool isLocked, string password = null)
    {
        // 룸 프로퍼티에 들어가야할 것들 : 시간, 목숨, 시작 건파우더, 시간 당 감소
        Hashtable roomProperties = new Hashtable
        {
            {ERoomProperties.RoomName.ToString(), roomName},
            {ERoomProperties.MapSelected.ToString(), (int)InitialMap},
            {ERoomProperties.PlayTime.ToString(), playTime},
            {ERoomProperties.Life.ToString(), life},
            {ERoomProperties.Gunpowder.ToString(), gunpowder},
            {ERoomProperties.DeclinePowder.ToString(), decline},
            {ERoomProperties.IsLocked.ToString(),isLocked },
            {ERoomProperties.Password.ToString(), password}
        };
        
        RoomOptions roomOptions = new RoomOptions();
        // 룸 세팅
        roomOptions.MaxPlayers = maxPlayers;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        roomOptions.CustomRoomPropertiesForLobby = SetRoomPropertiesForLobby();
        roomOptions.CustomRoomProperties = roomProperties; 
        roomOptions.EmptyRoomTtl = 0;
        
        string room = PhotonNetwork.LocalPlayer.UserId + " " + roomName;
        //방 만들기
        PhotonNetwork.CreateRoom(room,roomOptions, TypedLobby.Default);
    }
    
    private string[] SetRoomPropertiesForLobby()
    {
        Array enumValues = Enum.GetValues(typeof(ERoomProperties));
        int count = enumValues.Length -1;
        
        string[] roomProperties = new string[count];
        
        for (int i = 0; i < count; i++)
        {
            roomProperties[i] = enumValues.GetValue(i).ToString();
        }
        
        return roomProperties;
    }
    // 계정 정보들 가져오기?
    // 룸 추가, 삭제
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (PhotonNetwork.InLobby == false)
        {
            return;
        }
        
        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList)
            {
                _roomInfoList.RemoveAll(x => x.Name == roomInfo.Name);
            }
            else
            {
                int index = _roomInfoList.FindIndex(x => x.Name == roomInfo.Name);
                if (index >= 0)
                {
                    _roomInfoList[index] = roomInfo;
                }
                else
                {
                    _roomInfoList.Add(roomInfo);
                }
            }

        }

        EventManager.Instance.RoomListUpdate();
    }
}
