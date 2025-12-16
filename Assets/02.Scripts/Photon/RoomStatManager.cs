using System;
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
    public int PlayerGunpowder;
    public int PlayerDecreaseTime;
    public EInGameTeam PlayerTeam;
    public bool CanUlti = true;
    
    [SerializeField] private bool _isManual = false;
    [SerializeField] private bool _infiniteLife = false;

    
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
        PlayerDecreaseTime = (int)(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.DeclinePowder.ToString()]);
        
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(EProperties.Team.ToString()) == false)
        {
            return;
        }
        
        int team = Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()]);
        
        PlayerTeam = (EInGameTeam)team;

    }
    
    
}
