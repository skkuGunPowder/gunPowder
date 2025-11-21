using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MaskTransitions;
using UnityEngine.SceneManagement;

public class UI_RoomSlot : MonoBehaviour
{
    public float ClickInterval = 0.5f;
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
    public RoomInfo _roomInfo;
    
    private bool _isLocked = false;
    private bool _isClicked = false;
    public void Refresh(Sprite mapIcon, RoomInfo roomInfo)
    {
        _isClicked = false;
        _roomInfo = roomInfo;
        
        RoomName.text = roomInfo.CustomProperties[ERoomProperties.RoomName.ToString()].ToString();
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

    public void OnClicked()
    {
        if (_isClicked) // 중복 클릭 방지
        {
            return;
        }
        
        StartCoroutine(OnClick_Coroutine());
        
        // 잠겨있는 방인가?
        if(_isLocked)
        {
            UI_PasswordPopup password = (UI_PasswordPopup)PopupManager.Instance.Open(EPopupType.UI_PasswordPopup);
            password.SetRoomInfo(_roomInfo);
            return;
        }   
        
        // 풀방인가?
        if (_roomInfo.PlayerCount >= _roomInfo.MaxPlayers)
        {
            UI_MessagePopup message = (UI_MessagePopup)PopupManager.Instance.Open(EPopupType.UI_MessagePopup);
            message.Init("방이 가득 찼습니다", false);
            return;
        }
        
        TransitionManager.Instance.StartAnimation(0.3f);
        PhotonNetwork.JoinRoom(_roomInfo.Name);
    }

    private IEnumerator OnClick_Coroutine()
    {
        _isClicked = true;
        yield return new WaitForSeconds(ClickInterval);
        _isClicked = false;

    }

    private void OnDisable()
    {
        StopCoroutine(OnClick_Coroutine());
    }
}

