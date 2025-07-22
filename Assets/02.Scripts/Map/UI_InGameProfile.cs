using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class UI_InGameProfile : MonoBehaviour
{ 
    [SerializeField]
    private List<UI_InGameProfileSlot> UI_InGameProfileSlotList = new List<UI_InGameProfileSlot>();
    
    private void Awake()
    {
        PlayerSettingManager.Instance.OnDataChanged += Refresh;
    }

    private void Start()
    {
        List<int> playerNumberList = new List<int>();

        playerNumberList = PlayerSettingManager.Instance.PlayerList;

        for (int i = 0; i < UI_InGameProfileSlotList.Count; i++)
        {
            if (i < playerNumberList.Count)
            {
                UI_InGameProfileSlotList[i].Init(playerNumberList[i]);
                UI_InGameProfileSlotList[i].Refresh(PlayerSettingManager.Instance.Gunpowder, PlayerSettingManager.Instance.Life);   
            }
            else
            {
                UI_InGameProfileSlotList[i].gameObject.SetActive(false);
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
