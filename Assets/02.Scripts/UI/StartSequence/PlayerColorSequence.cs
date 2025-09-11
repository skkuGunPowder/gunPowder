using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class PlayerColorSequence : MonoBehaviour
{
    public List<TeamColorSetting> TeamColorSettingList;
    
    private void Start()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
    }
}
