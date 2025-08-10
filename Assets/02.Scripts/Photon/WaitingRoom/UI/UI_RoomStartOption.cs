using System;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RoomStartOption : MonoBehaviour
{
    public Button MapSelectButton;
    public Image MapIcon;
    public TextMeshProUGUI MapNameGUGI;

    public List<MapDataSO> MapDataList;
    public Dictionary<string, MapDataSO> MapDataDictionary;
    
    
    private void Awake()
    {
        MapDataDictionary = new Dictionary<string, MapDataSO>();
        
        foreach (MapDataSO dataSo in MapDataList)
        {
            MapDataDictionary.Add(dataSo.MapSceneList.ToString(), dataSo);
        }

        EventManager.Instance.OnMapChanged += MapChange;
        EventManager.Instance.OnMasterChanged += ButtonSetup;

    }

    private void Start()
    {
        ButtonSetup();
    }
    
    // 탭 선택하기 => 고른 옵션만 true 나머지는 false
    // 맵 선택 interation 조절 => 방장만 누를 수 있게
    public void ButtonSetup()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            MapSelectButton.interactable = true;
        }
        else
        {
            MapSelectButton.interactable = false;
        }
    }
    
    // 맵 설정 하기
    public void MapChange()
    {
        ESceneList map = RoomManager.Instance.SelectedMap;
        
        string name = MapDataDictionary[map.ToString()].MapName;
        Sprite mapSprite = MapDataDictionary[map.ToString()].MapSprite;
        
        Refresh(name, mapSprite);
    }

    private void Refresh(string mapName, Sprite map)
    {
        MapNameGUGI.text = mapName;
        MapIcon.sprite = map;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnMapChanged -= MapChange;
        EventManager.Instance.OnMasterChanged -= ButtonSetup;
    }
}
