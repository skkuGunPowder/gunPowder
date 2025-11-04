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
        LifeSetting();
        PlayerGunpowder = (int)(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.Gunpowder.ToString()]); 
        PlayerDecreaseTime = (int)(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.DeclinePowder.ToString()]);
        
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(EProperties.Team.ToString()) == false)
        {
            return;
        }
        
        int team = Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()]);
        
        PlayerTeam = (EInGameTeam)team;

    }

    private void LifeSetting()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.Life.ToString()) == false)
        {
            PlayerLife = 3;
            return;
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.GameMode.ToString()) && PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.GameMode.ToString()] != null)
        {
            EGameMode mode = (EGameMode)PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.GameMode.ToString()];

            if (mode == EGameMode.Infinite)
            {
                PlayerLife = 1000;
                return;
            }
        }
        
        PlayerLife = (int)(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.Life.ToString()]);
    }
    
    
}
