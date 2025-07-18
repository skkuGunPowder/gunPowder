using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RoomMakerPopup : UI_Popup
{
    public TMP_InputField RoomName;
    public TMP_InputField RoomPassword;
    
    // public 
    public Toggle IsLocked;            // 비번 방 여부
    // public 
    public TMP_Dropdown MaxPlayers;
    public UI_SetupButton PlayTime;
    public UI_SetupButton Life;
    public UI_SetupButton Gunpowder;
    public UI_SetupButton Decline;
    
    public List<UI_SetupButton> SetupButtonList = new List<UI_SetupButton>();
    public void OnclickCreateRoom()
    {
        string roomName = RoomName.text;
        int maxPlayers = int.Parse(MaxPlayers.options[MaxPlayers.value].text);
        
        Debug.Log(maxPlayers);
        if (IsLocked.isOn == false)
        {
            RoomPassword.text = "";
        }
        
        LobbyManager.Instance.MakeRoom(roomName, maxPlayers, PlayTime.CurrentValue(), Life.CurrentValue(),
            Gunpowder.CurrentValue(), Decline.CurrentValue(), IsLocked.isOn, RoomPassword.text);
    }
    
    public void Cancel()
    {
        gameObject.SetActive(false);    
    }
    
    // UI 초기화하기
    public void OnDisable()
    {
        RoomName.text = "";
        RoomPassword.text = "";
        MaxPlayers.value = 0;
    }

    public void LockedButton()
    {
        RoomPassword.interactable = IsLocked.isOn;
    }
}
