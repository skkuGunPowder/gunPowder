using System;
using Photon.Pun;
using UnityEngine;

public class RoomStatManager : Singleton<RoomStatManager>
{
    // 스탯 뿌려주는 매니저 = 플레이어에게 뿌려줄 스탯을 찾아옴
    public int PlayerLife;
    public int PlayerGunpowder;
    public int PlayerDecreaseTime;
    public EInGameTeam PlayerTeam;
    
    protected override void Awake()
    {
        base.Awake();
        
        PlayerLife = (int)(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.Life.ToString()]);
        PlayerGunpowder = (int)(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.Gunpowder.ToString()]); 
        PlayerDecreaseTime = (int)(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.DeclinePowder.ToString()]);
        int team = Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()]);
        
        PlayerTeam = (EInGameTeam)team;

    }
    
    
}
