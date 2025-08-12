using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class PhotonTest2 : PhotonSingleton<PhotonTest2>
{
    [SerializeField] private PhotonView _photonView;

    private void Start()
    {
        if (_photonView.IsMine)
        {
            this.gameObject.SetActive(true);   
        }
        else
        {
            this.gameObject.SetActive(false);   
        }
    }
}
