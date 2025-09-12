using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class SkinSettingForUlti : MonoBehaviour
{
    public List<ProfileColorChange> ColorChangeList = new List<ProfileColorChange>();
    public List<PlayerStartSkin> StartSkinList = new List<PlayerStartSkin>();
    public List<SkinAnimationController> SkinAnimationControllerList = new List<SkinAnimationController>();
    
    public void Init()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < StartSkinList.Count; i++)
        {
            if (i < players.Length)
            {
                StartSkinList[i].Refresh(players[i]);
                ColorChangeList[i].Refresh(players[i]);
                SkinAnimationControllerList[i].Init();
                StartSkinList[i].gameObject.SetActive(false);   
            }
            else
            {
                StartSkinList[i].gameObject.SetActive(false);
            }
        }
    }
}
