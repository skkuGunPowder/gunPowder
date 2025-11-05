using System;
using Photon.Pun;
using UnityEngine;

public class RoomStatManager : Singleton<RoomStatManager>
{
    // 기본 스탯
    [Header("기본 값")]
    [SerializeField]private const int DefaultLife = 3;
    [SerializeField]private const int DefaultGunpowder = 100;
    [SerializeField] private const int DefaultInfiniteGunpowder = 70;
    [SerializeField] private const int DefaultInfiniteLife = 1000;
    [SerializeField] private const EInGameTeam DefaultInGameTeam = EInGameTeam.Red;
    // 스탯 뿌려주는 매니저 = 플레이어에게 뿌려줄 스탯을 찾아옴
    public int PlayerLife;
    public int PlayerGunpowder;
    public int PlayerDecreaseTime;
    public EInGameTeam PlayerTeam;
    
    protected override void Awake()
    {
        base.Awake();
        LifeSetting();
        GunpowderSetting();
        PlayerDecreaseTime = (int)(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.DeclinePowder.ToString()]);
        
        TeamSetting();
    }
    
    private void TeamSetting()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(EProperties.Team.ToString()) == false)
        {
            PlayerTeam = DefaultInGameTeam;
            return;
        }
        
        int team = Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()]);
        
        PlayerTeam = (EInGameTeam)team;

    }
    
    private void LifeSetting()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.GameMode.ToString()) 
            && PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.GameMode.ToString()] != null)
        {
            EGameMode mode = (EGameMode)PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.GameMode.ToString()];

            if (mode == EGameMode.Infinite)
            {
                PlayerLife = DefaultInfiniteLife;
                return;
            }
        }
        
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.Life.ToString()) == false)
        {
            PlayerLife = DefaultLife;
            return;
        }
        
        PlayerLife = (int)(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.Life.ToString()]);
    }
    
    private void GunpowderSetting()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.GameMode.ToString()) 
            && PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.GameMode.ToString()] != null)
        {
            EGameMode mode = (EGameMode)PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.GameMode.ToString()];

            if (mode == EGameMode.Infinite)
            {
                PlayerGunpowder = DefaultInfiniteGunpowder;
                return;
            }
        }
        
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.Gunpowder.ToString()) == false)
        {
            PlayerGunpowder = DefaultGunpowder;
            return;
        }
        
        PlayerGunpowder = (int)(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.Gunpowder.ToString()]);
    }
    
}
