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
    
    private void Awake()
    {
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
    public void MapChange(EMap map)
    {
        MapData data = MapDataManager.Instance.GetDataLoad(map);

        string mapName = data.MapName;
        Sprite mapSprite = data.MapSprite;
        
        Refresh(mapName, mapSprite);
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
