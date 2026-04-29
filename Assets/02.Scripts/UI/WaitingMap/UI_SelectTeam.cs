using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class UI_SelectTeam : MonoBehaviour
{
    public List<UI_TeamSelcetButton> TeamButtonList = new List<UI_TeamSelcetButton>();

    private void Awake()
    {
        EventManager.Instance.OnTeamChanged += TeamSelected;
    }

    private void Start()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(EProperties.Team.ToString()) == false)
        {
            return;
        }
        
        EInGameTeam team = (EInGameTeam)PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()];        
        
        foreach (UI_TeamSelcetButton teamButton in TeamButtonList)
        {
            teamButton.Refresh(team);
        }
        
    }
    private void TeamSelected()
    {
        EInGameTeam team = RoomManager.Instance.SelectedTeam;
        
        foreach (UI_TeamSelcetButton teamButton in TeamButtonList)
        {
            teamButton.Refresh(team);
        }
    }

    private void OnDisable()
    {
        EventManager.Instance.OnTeamChanged -= TeamSelected;
    }
}
