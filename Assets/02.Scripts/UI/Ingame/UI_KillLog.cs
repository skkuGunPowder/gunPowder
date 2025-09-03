using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class UI_KillLog : MonoBehaviour
{
    // 슬롯 리스트를 가지고 있다.
    public List<UI_KillLogSlot> KillLogSlotList = new List<UI_KillLogSlot>();
    private List<PhotonPlayer> _playerList = new List<PhotonPlayer>();
    private int _myTeam;
    // 슬롯 리스트들 확인해서 현재 사용중인 슬롯인가 체크
    
    private void Awake()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(EProperties.Team.ToString()) == false)
        {
            return;
        }
        
        _myTeam = (int)PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()];
    }

    private void OnEnable()
    {
        EventManager.Instance.OnUpdateKillLog += Refresh;
        _playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
    }

    public void Refresh(int kill, int death)
    {
        foreach (UI_KillLogSlot slot in KillLogSlotList)
        {
            if (slot.gameObject.activeInHierarchy)
            {
                continue;
            }

            string killPlayer = GetPlayerNickName(kill);
            string deathPlayer = GetPlayerNickName(death);
            
            bool killerTeam = TeamCheck(kill);
            bool deathTeam = TeamCheck(death);
            
            slot.gameObject.SetActive(true); 
            slot.Refresh(killPlayer, deathPlayer, killerTeam, deathTeam);
            break;
        }
    }

    private string GetPlayerNickName(int playerNumber)
    {
        foreach (PhotonPlayer player in _playerList)
        {
            if (player.ActorNumber == playerNumber)
            {
                return player.NickName;
            }
        }

        return "";
    }

    private bool TeamCheck(int playerNumber)
    {
        foreach (PhotonPlayer player in _playerList)
        {
            if (player.ActorNumber == playerNumber)
            {
                int playerTeam = (int)player.CustomProperties[EProperties.Team.ToString()];

                if (playerTeam == _myTeam)
                {
                    return true;
                }
            }
        } 
        return false;
    }
    private void OnDisable()
    {
        EventManager.Instance.OnUpdateKillLog -= Refresh;
    }
    // 사용중인 것들 모두 건너 뛰고 다음 꺼 사용
    
}
