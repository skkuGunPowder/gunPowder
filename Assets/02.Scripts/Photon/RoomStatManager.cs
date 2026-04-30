using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class RoomStatManager : Singleton<RoomStatManager>
{
    // 스탯 뿌려주는 매니저 = 플레이어에게 뿌려줄 스탯을 찾아옴
    /// <summary>
    /// 플레이어 스탯 초기화
    /// isMaual = true라면 수동으로 설정한 값을 따름
    /// </summary>
    
    public int PlayerLife;
    public int PlayerGunpowder; // GP 초기값 (재화)
    public int PlayerDecreaseTime;

    public int InitGunpowder; // 디폴트값
    
    public EInGameTeam PlayerTeam;
    public bool CanUlti = true;
    public const int PlayerHP = 150; // HP 고정값
    
    private const int ADDITIONAL_GUNPOWDER = 30;
    [SerializeField] private bool _isManual = false;
    [SerializeField] private bool _infiniteLife = false;

    private bool _initialized = true;
    
    protected override void Awake()
    {
        base.Awake();

        if (_isManual)
        {
            if (_infiniteLife)
            {
                PlayerLife = int.MaxValue;
            }
            return;
        }
        
        PlayerLife = (int)(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.Life.ToString()]);
        PlayerGunpowder = (int)(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.Gunpowder.ToString()]);
        InitGunpowder = PlayerGunpowder;
        
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(EProperties.Team.ToString()) == false)
        {
            return;
        }
        
        int team = Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()]);
        
        PlayerTeam = (EInGameTeam)team;

    }

    public bool CanChangeGP(int gp)
    {
        return PlayerGunpowder + gp >= 0;
    }
    public void ChangeGunpowder(int myGunpowder)
    {
        PlayerGunpowder += myGunpowder;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
        {
            { EProperties.GP.ToString(), PlayerGunpowder }
        });
    }

    public void SetGunpowder(int myGunpowder)
    {
        PlayerGunpowder = myGunpowder;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
        {
            { EProperties.GP.ToString(), PlayerGunpowder }
        });
    }
    
    public int GetGunpowder()
    {
        if (_initialized == false)
        {
            if (PlayerGunpowder < 0)
            {
                PlayerGunpowder = ADDITIONAL_GUNPOWDER;
                
            }
            else
            {
                PlayerGunpowder += ADDITIONAL_GUNPOWDER;
            }
            
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
            {
                { EProperties.GP.ToString(), PlayerGunpowder }
            });
            
            return PlayerGunpowder;
        }
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
        {
            { EProperties.GP.ToString(), PlayerGunpowder }
        });
        
        _initialized = false;
        return PlayerGunpowder;
    }
    
}
