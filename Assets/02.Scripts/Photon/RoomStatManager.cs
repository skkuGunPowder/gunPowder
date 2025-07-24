using System;
using Photon.Pun;
using UnityEngine;

public class RoomStatManager : Singleton<RoomStatManager>
{
    public int PlayerLife;
    public int PlayerGunpowder;

    protected override void Awake()
    {
        base.Awake();
        
        PlayerLife = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Life}"].ToString());
        PlayerGunpowder = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Gunpowder}"].ToString()); 
    }
    
    
}
