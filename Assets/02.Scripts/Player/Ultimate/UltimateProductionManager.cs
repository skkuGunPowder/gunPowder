using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class UltimateProductionManager : MonoBehaviour
{   
    public List<UltimateProductionSlot> UltimateProductionSlotList;

    private void Awake()
    {
        EventManager.Instance.OnUltimate += Play;
    }

    private void Play(string bomb, PhotonPlayer player)
    {
        foreach (UltimateProductionSlot slot in UltimateProductionSlotList)
        {
            if (slot.gameObject.activeSelf == false)
            {
                slot.gameObject.SetActive(true);
                bool teamCheck = TeamCheck(player);
                slot.Play(bomb, teamCheck);
                break;
            }
        }
    }

    private bool TeamCheck(PhotonPlayer player)
    {
        EInGameTeam myTeam = (EInGameTeam)PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()];
        EInGameTeam OtherTeam = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
        
        if(myTeam == OtherTeam)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDisable()
    {
        EventManager.Instance.OnUltimate -= Play;
    }
}
