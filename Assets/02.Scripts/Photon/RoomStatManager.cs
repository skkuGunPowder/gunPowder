using System;
using Photon.Pun;
using UnityEngine;

public class RoomStatManager : Singleton<RoomStatManager>
{
    // 스탯 뿌려주는 매니저 = 플레이어에게 뿌려줄 스탯을 찾아옴
    public int PlayerLife;
    public int PlayerGunpowder;
    public int PlayerDecreaseTime;
    
    protected override void Awake()
    {
        base.Awake();
        
        PlayerLife = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Life}"].ToString());
        PlayerGunpowder = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.Gunpowder}"].ToString()); 
        PlayerDecreaseTime = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[$"{EProperties.DeclinePowder}"].ToString());
    }
    
    
}
