using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class UI_MapSetup : MonoBehaviour
{
    public ESceneList SelectedMap;
    
    public void SetupMap()
    {
        RoomManager.Instance.SelectedMap = SelectedMap;
        
        Hashtable roomProperties = new Hashtable()
        {
            {"MapSelected", SelectedMap}
        };
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        
        Debug.Log("Selected Map: " + RoomManager.Instance.SelectedMap);
    }
}
