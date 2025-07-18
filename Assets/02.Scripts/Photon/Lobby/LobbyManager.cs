using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager Instance;
    private List<RoomInfo> _roomInfoList = new List<RoomInfo>();
    public List<RoomInfo> RoomInfoList => _roomInfoList;

    public event Action OnDataChanged;
    public event Action OnMapChanged;
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
        Hashtable roomProperties = new Hashtable
        {
            {"MapSelected", ESceneList.Map1.ToString()}
        };
        
        RoomOptions roomOptions = new RoomOptions();
        
        roomOptions.MaxPlayers = maxPlayers;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        roomOptions.CustomRoomPropertiesForLobby = new string[] {"MapSelected"};
        roomOptions.CustomRoomProperties = roomProperties; 
        roomOptions.EmptyRoomTtl = 0;
        
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }
    // 모든 룸 정보들을 가지고 있어야함 => 방 refresh담당
    
    // 계정 정보들 가져오기?
    // 룸 추가, 삭제
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log(roomList.Count);
        Debug.Log(_roomInfoList.Count);
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
            
            if (roomInfo.CustomProperties.ContainsKey("MapSelected"))
            {
                string mapName = roomInfo.CustomProperties["MapSelected"].ToString();
                Debug.Log($"방 이름: {roomInfo.Name}, 맵: {mapName}");
            }
        }
        
        
        OnDataChanged?.Invoke();
    }

    // public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    // {
    //     Debug.Log("OnRoomPropertiesUpdate");
    //     Debug.Log($"{propertiesThatChanged["SelectedMap"]}");
    //     if (propertiesThatChanged.ContainsKey("SelectedMap") && propertiesThatChanged["SelectedMap"] != null)
    //     {
    //         OnDataChanged?.Invoke();
    //     }
    // }
    
}
