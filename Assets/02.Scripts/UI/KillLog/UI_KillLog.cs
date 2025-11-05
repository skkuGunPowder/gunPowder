using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class UI_KillLog : MonoBehaviour
{
    // 슬롯 리스트를 가지고 있다.
    public List<UI_KillLogSlot> KillLogSlotList = new List<UI_KillLogSlot>();
    public UI_KillPannel KillPannel;
    // 현재 플레이어별 팀
    private Dictionary<PhotonPlayer, int> _playerTeamDictionary = new Dictionary<PhotonPlayer, int>();
  
    // 내 정보
    private PhotonPlayer _myPlayer;
    private int _myTeam;
    
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
        Init();
    }

    private void Init()
    {
        _myPlayer = PhotonNetwork.LocalPlayer;
        _playerTeamDictionary.Clear();
        
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        
        foreach (PhotonPlayer player in players)
        {
            int team = (int)player.CustomProperties[EProperties.Team.ToString()];
            _playerTeamDictionary.Add(player, team);
        }
    }
    private void Refresh(int kill, bool isNormal, int death)
    {
        if (kill != death && kill == _myPlayer.ActorNumber)
        {
            KillPannelLog(death);
        }
        
        foreach (UI_KillLogSlot slot in KillLogSlotList)
        {
            if (slot.gameObject.activeInHierarchy) // 킬 슬롯이 사용인지 체크
            {
                continue;
            }
            
            bool killerTeam = TeamCheck(kill);
            bool deathTeam = TeamCheck(death);
            
            slot.gameObject.SetActive(true); 
            slot.Refresh(kill, death, killerTeam, deathTeam, isNormal);
            break;
        }
    }

    private void KillPannelLog(int death)
    {
        PhotonPlayer player = PhotonNetwork.CurrentRoom.GetPlayer(death);
        
        KillPannel.gameObject.SetActive(true);
        KillPannel.Refresh(player.NickName);
    }
    
    private bool TeamCheck(int playerNumber)
    {
        PhotonPlayer player = PhotonNetwork.CurrentRoom.GetPlayer(playerNumber);
        
        int playerTeam = _playerTeamDictionary[player];
        
        if (playerTeam == _myTeam)
        {
            return true;
        }
        
        return false;
    }
    private void OnDisable()
    {
        EventManager.Instance.OnUpdateKillLog -= Refresh;
    }
    
}
