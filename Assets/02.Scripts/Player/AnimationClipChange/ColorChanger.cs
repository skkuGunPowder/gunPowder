using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    public PhotonView MyPhotonView;
    public List<TeamColorSetting> TeamColorSettingList;
    private List<ColorChanger> _currentPlayerList = new List<ColorChanger>();
    private void Awake()
    {
        MyPhotonView = GetComponent<PhotonView>();
    }
    
    private void OnEnable()
    {
        ColorChanger[] players= FindObjectsByType<ColorChanger>(FindObjectsSortMode.None);
        _currentPlayerList = new List<ColorChanger>(players);
        
        OtherPlayerColorChange();
        
        EventManager.Instance.OnPlayerColorChanged += RefreshRequest;
    }
    
    // 모든 플레이어의 색을 변경한다. = 초기화
    private void OtherPlayerColorChange()
    {
        foreach (ColorChanger color in _currentPlayerList)
        {
            if (color.MyPhotonView.Owner.CustomProperties.ContainsKey(EProperties.Team.ToString()) == false)
            {

                foreach (var colorSetting in color.TeamColorSettingList)
                {
                    colorSetting.Refresh(EInGameTeam.Red);
                }
                
                continue;
            };
            
            EInGameTeam team = (EInGameTeam)color.MyPhotonView.Owner.CustomProperties[EProperties.Team.ToString()];
           
            foreach (var colorSetting in color.TeamColorSettingList)
            {
                colorSetting.Refresh(team);
            }
            
        }
    }
    
    // 팀을 바꾼 플레이어를 찾아서 그 플레이어의 색을 변경해줌
    private void RefreshRequest(int player, EInGameTeam team)
    {
        if (MyPhotonView.IsMine == false)
        {
            return;
        }

        _currentPlayerList.Clear();

        ColorChanger[] players = FindObjectsByType<ColorChanger>(FindObjectsSortMode.None);
        _currentPlayerList = new List<ColorChanger>(players);
        foreach (ColorChanger color in _currentPlayerList)
        {
            if (color.MyPhotonView.OwnerActorNr == player)
            {
                foreach (var colorSetting in color.TeamColorSettingList)
                {
                    colorSetting.Refresh(team);
                }
                break;
            }
        }
    }

    private void OnDisable()
    {
        EventManager.Instance.OnPlayerColorChanged -= RefreshRequest;
    } 
}

