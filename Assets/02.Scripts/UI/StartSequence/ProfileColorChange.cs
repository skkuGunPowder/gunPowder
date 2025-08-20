using UnityEngine;
using System.Collections.Generic;
public class ProfileColorChange : MonoBehaviour
{
    public List<TeamColorSetting> TeamColorSettingList;

    public void Refresh(EInGameTeam team)
    {
        foreach (var color in TeamColorSettingList)
        {
            color.Refresh(team);
        }
    }
}

