using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class UI_TeamSelcetButton : MonoBehaviour
{
    public EInGameTeam MyTeam;

    public void OnClickTeamSelect()
    {
        Hashtable team = new Hashtable()
        {
            {EProperties.Team.ToString(), (int)MyTeam}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(team);
        
        Debug.Log(PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()]);
    }
    
}
