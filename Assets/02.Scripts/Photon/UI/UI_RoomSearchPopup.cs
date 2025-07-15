using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class UI_RoomSearchPopup : UI_Popup
{
    public List<UI_RoomSlot> RoomSlotList;

    private int _currentPage = 1;
    private int _maxPage;
    
    private void Awake()
    {
        LobbyManager.Instance.OnDataChanged += Refresh;
    }

    private void Start()
    {
        Refresh();
    }

    // 방이 보일 수 있을만큼만 보여준다 (최대 8개) => refresh할 것들 : 현재 있는 방의 개수, 방 페이지
    public void Refresh()
    {
        List<RoomInfo> roomInfoList = LobbyManager.Instance.RoomInfoList;

        if (roomInfoList == null || roomInfoList.Count == 0)
        {
            foreach (UI_RoomSlot slot in RoomSlotList)
            {
                slot.gameObject.SetActive(false);
            }
            return;
        }
        int roomCount = roomInfoList.Count;
        _maxPage = (int)(roomCount / RoomSlotList.Count + 1);
        
        
        int startIndex = (_currentPage - 1) * RoomSlotList.Count;
        
        int countThisPage = Mathf.Min(RoomSlotList.Count, roomCount - startIndex);
        
        for (int i = 0; i < RoomSlotList.Count; i++)
        {
            if (i < countThisPage)
            {
                RoomInfo room = roomInfoList[startIndex + i];
                RoomSlotList[i].gameObject.SetActive(true);
                RoomSlotList[i].Refresh(room.Name, room.PlayerCount, room.MaxPlayers);
            }
            else
            {
                RoomSlotList[i].gameObject.SetActive(false);
            }
        }
    }
    
}
