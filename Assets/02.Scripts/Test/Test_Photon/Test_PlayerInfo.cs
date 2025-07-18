using Photon.Pun;
using UnityEngine;

public class Test_PlayerInfo : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Life}"]);
        Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.IsLocked}"]);
        Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Gunpowder}"]);
        Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.DeclinePowder}"]);
        Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Password}"]);
        Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.PlayTime}"]);
    }
}
