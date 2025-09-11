using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class SequenceColorChanger : MonoBehaviour
{
    public List<ProfileColorChange> TeamColorSettingList;
    private List<PhotonPlayer> _playerList = new List<PhotonPlayer>();
    private void Start()
    {
        _playerList.Clear();
        _playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);

        for (int i = 0; i < _playerList.Count; i++)
        {
            TeamColorSettingList[i].Refresh(_playerList[i]); 
        }
    }
}
