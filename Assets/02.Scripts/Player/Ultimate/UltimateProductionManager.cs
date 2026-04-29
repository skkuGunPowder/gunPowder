using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class UltimateProductionManager : MonoBehaviour
{   
    public Dictionary<int, UltimateProductionSlot> UltimateProductionSlotDic = new Dictionary<int, UltimateProductionSlot>();
    public List<UltimateProductionSlot> UltimateProductionSlotList;
    public SkinSettingForUlti SkinSettingForUlti;
    private EInGameTeam _myTeam;
    private void Awake()
    {
        EventManager.Instance.OnUltimate += Play;
    }

    private void Start()
    {
        _myTeam = (EInGameTeam)PhotonNetwork.LocalPlayer.GetCustomProperty<int>(EProperties.Team.ToString());
        SkinSettingForUlti.Init();
        Init();
    }

    private void Init()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {

            UltimateProductionSlotList[i].AddPlayer(SkinSettingForUlti.StartSkinList[i].gameObject);
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
