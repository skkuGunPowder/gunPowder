using System;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UI_RoomSlot : MonoBehaviour
{
    public TextMeshProUGUI RoomName;
    public TextMeshProUGUI PlayerCount;
    public Image MapIcon;
    public UI_PasswordPopup PasswordPopup;
    private RoomInfo _roomInfo;
    
    public void Refresh(string roomName, int currentPlayerCount, int maxPlayerCount, Sprite mapIcon, RoomInfo roomInfo)
    {
        Debug.Log($"Refresh : {mapIcon.name}");
        RoomName.text = roomName;
        PlayerCount.text = $"{currentPlayerCount}/{maxPlayerCount}";
        MapIcon.sprite = mapIcon;
        _roomInfo = roomInfo;
    }
    
    public void PasswordCheck()
    {
        if((bool)_roomInfo.CustomProperties[$"{EProperties.IsLocked}"])
        {
            PopupManager.Instance.Open(EPopupType.UI_PasswordPopup);
        }
        else
        { 
            JoinRoom();
        }
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(RoomName.text);
    }

    public void CloseRoom()
    {
        string password = _roomInfo.CustomProperties[$"{EProperties.Password}"].ToString();
        
        if (PasswordPopup.PasswordCheck(password))
        {
            JoinRoom();
            PopupManager.Instance.Close(EPopupType.UI_PasswordPopup);
        }
        else
        {
            
        }
    }
    
}
