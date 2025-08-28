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
    
    private bool _isLocked = false;
    
    public void Refresh(Sprite mapIcon, RoomInfo roomInfo)
    {
        _roomInfo = roomInfo;
        
        RoomName.text = roomInfo.Name;
        PlayerCount.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        MapIcon.sprite = mapIcon;
        
        // 커스텀 프로퍼티가 필요한 요소
        IsRocked(ERoomProperties.IsLocked);
        GunpowderAmount.text = GetRoomProperties(ERoomProperties.Gunpowder);
        DeclineAmount.text = GetRoomProperties(ERoomProperties.DeclinePowder);
        LifeAmount.text = GetRoomProperties(ERoomProperties.Life);
        PlayTime.text = GetRoomProperties(ERoomProperties.PlayTime);
        
    }

    private string GetRoomProperties(ERoomProperties roomProperties)
    {
        return _roomInfo.CustomProperties[roomProperties.ToString()].ToString() ;   
    }

    private void IsRocked(ERoomProperties roomProperties)
    {
        _isLocked = (bool)_roomInfo.CustomProperties[roomProperties.ToString()];
        LockIcon.SetActive(_isLocked);
    }
    public void LockedCheck()
    {
        if(_isLocked)
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
