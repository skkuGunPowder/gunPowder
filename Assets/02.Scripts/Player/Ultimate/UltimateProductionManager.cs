using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class UltimateProductionManager : MonoBehaviour
{   
    public Dictionary<int, UltimateProductionSlot> UltimateProductionSlotDic = new Dictionary<int, UltimateProductionSlot>();
    public List<UltimateProductionSlot> UltimateProductionSlotList;
    public List<ProfileColorChange> ColorChangeList = new List<ProfileColorChange>();
    public List<PlayerStartSkin> StartSkinList = new List<PlayerStartSkin>();
    private EInGameTeam _myTeam;
    private void Awake()
    {
        EventManager.Instance.OnUltimate += Play;
    }

    private void Start()
    {
        _myTeam = (EInGameTeam)PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()];
        Init();
    }

    private void Init()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            StartSkinList[i].Refresh(players[i]);
            ColorChangeList[i].Refresh(players[i]);
            UltimateProductionSlotList[i].AddPlayer(StartSkinList[i].gameObject);
            StartSkinList[i].gameObject.SetActive(false);
            UltimateProductionSlotDic.Add(players[i].ActorNumber, UltimateProductionSlotList[i]);
        }
    }
    
    private void Play(string bomb, PhotonPlayer player)
    {
        UltimateProductionSlot slot = UltimateProductionSlotDic[player.ActorNumber];
        
        slot.gameObject.SetActive(true);
        slot.Play(bomb, TeamCheck(player));

    }
    private bool TeamCheck(PhotonPlayer player)
    {
        EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
        
        if(_myTeam == team)
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
