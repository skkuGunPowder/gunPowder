using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class UI_MapSetup : MonoBehaviour
{
    public EMap SelectedMap;

    public void SelectMap(EMap map)
    {
        SelectedMap = map;
    }
    public void SetupMap()
    {
        Hashtable roomProperties = new Hashtable()
        {
            {ERoomProperties.MapSelected.ToString(), (int)SelectedMap}
        };
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }
}
