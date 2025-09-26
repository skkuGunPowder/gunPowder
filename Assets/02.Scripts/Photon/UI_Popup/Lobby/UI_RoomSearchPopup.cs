using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class UI_RoomSearchPopup : UI_Popup
{
    [Header("현재 방")]
    public List<UI_RoomSlot> RoomSlotList;
    [Header("방 페이지 수")]
    public TextMeshProUGUI RoomPageTextUGUI;
    
    private int _currentPage = 1;
    private int _maxPage = 1;
    
    private void OnEnable()
    {
        EventManager.Instance.OnRoomListUpdate += Refresh;
        Refresh();
        PageSetting();
    }

    // 방이 보일 수 있을만큼만 보여준다 (최대 8개) => refresh할 것들 : 현재 있는 방의 개수, 방 페이지,
    public void Refresh()
    { 
        List<RoomInfo> roomInfoList = PhotonServerManager.Instance.RoomInfoList;
        
        // 방이 0개인 경우
        if (roomInfoList == null || roomInfoList.Count == 0)
        {
            foreach (UI_RoomSlot slot in RoomSlotList)
            {
                slot.gameObject.SetActive(false);
            }
            return;
        }
        
        // 페이지 수 설정
        int roomCount = roomInfoList.Count;
        _maxPage = (int)(roomCount / RoomSlotList.Count + 1);
        
        // 페이지 저장 인덱스
        int startIndex;
        
        if (_maxPage <= _currentPage)
        {
            startIndex = (_maxPage -1) * RoomSlotList.Count;
            _currentPage = _maxPage;
        }
        else
        {
            startIndex = (_currentPage - 1) * RoomSlotList.Count;
        }
        
        PageSetting();
        
        int countThisPage = Mathf.Min(RoomSlotList.Count, roomCount - startIndex);
        
        // 방 리프레시
        for (int i = 0; i < RoomSlotList.Count; i++)
        {
            if (i < countThisPage)
            {
                RoomInfo room = roomInfoList[startIndex + i];
                Sprite mapIcon = StringToSprite(room);
                RoomSlotList[i].gameObject.SetActive(true);
                RoomSlotList[i].Refresh(mapIcon, room);
            }
            else
            {
                RoomSlotList[i].gameObject.SetActive(false);
            }
        }
    }

    // 아이콘 가져오기
    private Sprite StringToSprite(RoomInfo info)
    {
        EMap map = (EMap)info.CustomProperties[ERoomProperties.MapSelected.ToString()];
        
        MapThemeData theme = MapDataManager.Instance.GetThemeData(map);
        
        if (theme == null)
        {
            return null;
        }
        
        return theme.MapIcon;
    }

    private void PageSetting()
    {
        RoomPageTextUGUI.text = $"{_currentPage}/{_maxPage}";
    }
    
    public void OnClickSetPage(int index)
    {
        _currentPage = Mathf.Clamp(_currentPage + index, 1, _maxPage);
        Refresh();
    }

    private void OnDisable()
    { 
        _currentPage = 1;
        EventManager.Instance.OnRoomListUpdate -= Refresh;
    }
}
