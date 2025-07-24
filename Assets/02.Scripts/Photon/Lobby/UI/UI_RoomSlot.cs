using System;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UI_RoomSlot : MonoBehaviour
{
    [Header("상단 정보")]
    public TextMeshProUGUI RoomName;
    public TextMeshProUGUI PlayerCount;
    
    [Header("하단 정보")]
    public TextMeshProUGUI GunpowderAmount;
    public TextMeshProUGUI DeclineAmount;
    public TextMeshProUGUI LifeAmount;
    public TextMeshProUGUI PlayTime;
    
    [Header("이미지")]
    public Image MapIcon;
    public GameObject LockIcon;
    
    [Header("참조")]
    public UI_PasswordPopup PasswordPopup;
    public RoomInfo _roomInfo;
    
    public void Refresh(string roomName, int currentPlayerCount, int maxPlayerCount, Sprite mapIcon, RoomInfo roomInfo)
    {
        RoomName.text = roomName;
        PlayerCount.text = $"{currentPlayerCount}/{maxPlayerCount}";
        MapIcon.sprite = mapIcon;
        _roomInfo = roomInfo;
        
        // 커스텀 프로퍼티가 필요한 요소
        GunpowderAmount.text = _roomInfo.CustomProperties[$"{EProperties.Gunpowder}"].ToString();
        DeclineAmount.text = _roomInfo.CustomProperties[$"{EProperties.DeclinePowder}"].ToString();
        LifeAmount.text = _roomInfo.CustomProperties[$"{EProperties.Life}"].ToString();
        PlayTime.text = _roomInfo.CustomProperties[$"{EProperties.PlayTime}"].ToString();
        LockIcon.SetActive((bool)_roomInfo.CustomProperties[$"{EProperties.IsLocked}"]);
    }
    
    public void LockedCheck()
    {
        Debug.Log($"{_roomInfo}");
        if((bool)_roomInfo.CustomProperties[$"{EProperties.IsLocked}"])
        {
            PopupManager.Instance.Open(EPopupType.UI_PasswordPopup);
            PasswordPopup.SetRoomInfo(_roomInfo);
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
    
}
