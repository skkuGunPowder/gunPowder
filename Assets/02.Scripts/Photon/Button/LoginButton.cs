using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class LoginButton : MonoBehaviour
{
    
    [SerializeField] private string _lobbyName;

    public void OnclickLogin()
    {
        PhotonNetwork.JoinLobby(); 
    }
}
