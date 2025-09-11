using UnityEngine;
using System.Collections.Generic;
using PhotonPlayer = Photon.Realtime.Player;
public class ProfileColorChange : MonoBehaviour
{
    public List<TeamColorSetting> TeamColorSettingList;

    public void Refresh(PhotonPlayer player)
    {
        EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
        foreach (var color in TeamColorSettingList)
        {
            color.Refresh(team);
        }
    }
}

