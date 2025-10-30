using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class LobbyManager : PhotonSingleton<LobbyManager>
{
    [Header("처음 맵 설정")] public EMap InitialMap = EMap.Forest1;
    
    protected override void Awake()
    {
        base.Awake();
        ClientManager.PlayBGM("Lobby");
    }

    // 방에 보내기
    public void MakeRoom(string roomName, int maxPlayers, int playTime, int life, int gunpowder, int decline, bool isLocked, string password = null)
    {
        // 채팅 채널 정보 생성 (Backend Chat SDK 길이 제한: 2~20자)
        // GUID 8자만 사용 (충분한 고유성 보장)
        string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        string chatChannelGroup = $"ingamechat";  
        string chatChannelId = $"rm{uniqueId}";
        // HashCode 사용 - Photon int 지원 + Backend Chat ulong 변환 가능 + 적절한 크기
        int chatChannelNumber = Math.Abs(uniqueId.GetHashCode());

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
            {ERoomProperties.Password.ToString(), password},
            {ERoomProperties.ChatChannelGroup.ToString(), chatChannelGroup},
            {ERoomProperties.ChatChannelId.ToString(), chatChannelId},
            {ERoomProperties.ChatChannelNumber.ToString(), chatChannelNumber}
        };
        
        RoomOptions roomOptions = new RoomOptions();
        // 룸 세팅
        roomOptions.MaxPlayers = maxPlayers;
        roomOptions.IsVisible = false;
        roomOptions.IsOpen = false;
        roomOptions.CustomRoomPropertiesForLobby = SetRoomPropertiesForLobby();
        roomOptions.CustomRoomProperties = roomProperties; 
        roomOptions.EmptyRoomTtl = 0;
        
        DateTime now = DateTime.Now;
        
        string room = PhotonNetwork.LocalPlayer.UserId + $"{now}" + roomName;
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
}
