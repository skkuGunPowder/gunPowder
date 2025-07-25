using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class UI_InGameProfile : MonoBehaviour
{ 
    [SerializeField]
    private List<UI_InGameProfileSlot> UI_InGameProfileSlotList = new List<UI_InGameProfileSlot>();
    private List<int> _playerActorNumberList = new List<int>();
    
    private void Awake()
    {
        PlayerSettingManager.Instance.OnDataChanged += Refresh;
        PlayerSettingManager.Instance.OnInitCharacter += Init;
        PlayerSettingManager.Instance.OnTopPlayerChanged += SetTopPlayer;
    }

    private void Init()
    {
        _playerActorNumberList = PlayerSettingManager.Instance.PlayerList;

        for (int i = 0; i < UI_InGameProfileSlotList.Count; i++)
        {
            if (i < _playerActorNumberList.Count)
            {
                // 후에 수정
                UI_InGameProfileSlotList[i].Init(_playerActorNumberList[i]);
                UI_InGameProfileSlotList[i].Refresh(RoomStatManager.Instance.PlayerGunpowder, RoomStatManager.Instance.PlayerLife);
            }
            else
            {
                UI_InGameProfileSlotList[i].gameObject.SetActive(false);
            }
        }
        
    }


    private void SetTopPlayer(int playerNumber)
    {
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            if (_playerActorNumberList[i] == playerNumber)
            {
                UI_InGameProfileSlotList[i].SetTop(true);
            }
            else
            {
                UI_InGameProfileSlotList[i].SetTop(false);
            }
        }

    }
    private void Refresh(int playerNumber, int gunpowder, int life)
    {
        List<int> playerNumberList = new List<int>();

        playerNumberList = PlayerSettingManager.Instance.PlayerList;

        for (int i = 0; i < playerNumberList.Count; i++)
        {
            if (playerNumberList[i] == playerNumber)
            {
                UI_InGameProfileSlotList[i].Refresh(gunpowder,life);
            }
        }

    }
}
