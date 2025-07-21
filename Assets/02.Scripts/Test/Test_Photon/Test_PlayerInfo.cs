using System;
using Photon.Pun;
using RaycastPro.RaySensors2D;
using UnityEngine;

public class Test_PlayerInfo : MonoBehaviour
{
    public BasicRay2D Ray2D;


    public void Update()
    {
        if (Ray2D.Cast())
        {
            
        }
    }

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
