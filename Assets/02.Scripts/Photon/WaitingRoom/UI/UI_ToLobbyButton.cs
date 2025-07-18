using Photon.Pun;
using UnityEngine;

public class UI_ToLobbyButton : MonoBehaviour
{
    public void OnClickToLobby()
    {
        PhotonNetwork.LeaveRoom();
        
    }
}
