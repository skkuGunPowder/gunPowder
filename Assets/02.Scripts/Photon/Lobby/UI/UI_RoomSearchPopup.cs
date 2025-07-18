using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class UI_RoomSearchPopup : UI_Popup
{
    public List<UI_RoomSlot> RoomSlotList;
    
    public TextMeshProUGUI RoomPageTextUGUI;
    
    public List<MapDataSO> MapDataList;
    public Dictionary<string, MapDataSO> MapDataDictionary;
    
    private int _currentPage = 1;
    private int _maxPage = 1;
    
    private void Awake()
    {
        LobbyManager.Instance.OnDataChanged += Refresh;
        
        MapDataDictionary = new Dictionary<string, MapDataSO>();
        
        foreach (MapDataSO dataSo in MapDataList)
        {
            MapDataDictionary.Add(dataSo.MapSceneList.ToString(), dataSo);
        }
    }

    private void Start()
    {
    }

    private void OnEnable()
    {
        Refresh();
        PageSetting();
    }

    // 방이 보일 수 있을만큼만 보여준다 (최대 8개) => refresh할 것들 : 현재 있는 방의 개수, 방 페이지,
    public void Refresh()
    { 
        Debug.Log("Refresh");
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
        
        for (int i = 0; i < RoomSlotList.Count; i++)
        {
            if (i < countThisPage)
            {
                RoomInfo room = roomInfoList[startIndex + i];
                Sprite mapIcon = StringToSprite(room);
                RoomSlotList[i].gameObject.SetActive(true);
                RoomSlotList[i].Refresh(room.Name, room.PlayerCount, room.MaxPlayers, mapIcon);
            }
            else
            {
                RoomSlotList[i].gameObject.SetActive(false);
            }
        }
    }

    private Sprite StringToSprite(RoomInfo info)
    {
        Debug.Log("맵 찾는중 ..");
        Debug.Log(info.CustomProperties["MapSelected"].ToString());
        string map = info.CustomProperties["MapSelected"].ToString();
        
        if (MapDataDictionary.TryGetValue(map, out var mapData))
        { 
            return mapData.MapSprite;
        }
        
        return null;
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
    }
}
