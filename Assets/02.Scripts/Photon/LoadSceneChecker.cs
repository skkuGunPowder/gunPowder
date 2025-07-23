using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

[RequireComponent(typeof(PhotonView))]
public class LoadSceneChecker : MonoBehaviourPunCallbacks
{
    private bool _isLoad = false;

    private void Start()
    {
        Hashtable load = new Hashtable()
        {
            { EProperties.IsLoad.ToString(), true }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(load);
    }
    
    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer ,Hashtable changedProps)
    {
        Debug.Log("좀 돼라");
        if (changedProps.ContainsKey(EProperties.IsDead.ToString()) && changedProps[EProperties.IsDead.ToString()] != null)
        {
            PlayerDeadCheck();
        }
        if (changedProps.ContainsKey(EProperties.IsLoad.ToString()) && changedProps[EProperties.IsLoad.ToString()] != null)
        {
            GameStart();
        }
    }
}
