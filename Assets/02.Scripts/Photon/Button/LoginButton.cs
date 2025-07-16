using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginButton : MonoBehaviour
{
    
    [SerializeField] private string _lobbyName;

    public void OnclickLogin()
    {
        PhotonServerManager.Instance.Connect();
    }
}
