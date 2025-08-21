using System;
using System.Collections.Generic;
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
        TeamSelected();
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
