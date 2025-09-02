using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class UI_MapSetup : MonoBehaviour
{
    public EMap SelectedMap;
    
    public void SetupMap()
    {
        RoomManager.Instance.SelectedMap = SelectedMap;
        
        Hashtable roomProperties = new Hashtable()
        {
            {ERoomProperties.MapSelected.ToString(), SelectedMap}
        };
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }
}
