using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class Indicator : MonoBehaviour
{
    [SerializeField] private PhotonView _photonView;
    
    private void Start()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentGameState == EGameState.Playing ||
            GameManager.Instance.CurrentGameState == EGameState.Ready)
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
}
