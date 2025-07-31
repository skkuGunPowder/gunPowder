using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class PhotonTest2 : PhotonSingleton<PhotonTest2>
{
    
    private bool _initialized = false;  // Init 한번만 부르게 하기
    
    public void Start()
    {
        int componentCount = GetComponents<RoomManager>().Length;
        Debug.Log($"컴포넌트 개수: {componentCount}");
        
        if (PhotonNetwork.InRoom == false)
        {
            return;
        }

        if (_initialized)
        {
            return;
        }
        
        Init();
    } 

    public override void OnJoinedRoom()
    {
        if (_initialized)
        {
            return;
        }
        
        base.OnJoinedRoom();
        Init();
    }

    public void Init()
    {
        _initialized = true;
        Hashtable hash = new Hashtable()
        {
            {"test", 1},
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer,Hashtable changedProps)
    {
        Debug.Log("OnPlayerPropertiesUpdate");
        if (changedProps.ContainsKey("test"))
        {
            Debug.Log(changedProps["test"]);
        }
    }
    
}
