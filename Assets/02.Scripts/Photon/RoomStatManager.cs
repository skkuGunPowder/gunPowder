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
    private const int NEGATIVE_GP_HP_PENALTY_CAP = 100; // 라운드 시작 시 음수 GP 페널티 HP 차감 캡
    [SerializeField] private bool _isManual = false;
    [SerializeField] private bool _infiniteLife = false;

    private bool _initialized = true;

    // 라운드 시작 시 GP가 음수였던 만큼 HP에서 차감할 페널티 (PlayerStat.Start가 1회 소비)
    private int _pendingNegativeGPPenalty = 0;
    
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
                // 라운드 시작 시 빚 만큼 HP 페널티 (캡 적용)을 다음 PlayerStat.Start가 소비
                _pendingNegativeGPPenalty = Mathf.Min(Mathf.Abs(PlayerGunpowder), NEGATIVE_GP_HP_PENALTY_CAP);
                PlayerGunpowder = ADDITIONAL_GUNPOWDER;
            }
            else
            {
                _pendingNegativeGPPenalty = 0;
                PlayerGunpowder += ADDITIONAL_GUNPOWDER;
            }

            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
            {
                { EProperties.GP.ToString(), PlayerGunpowder }
            });

            return PlayerGunpowder;
        }

        // 첫 매치 첫 호출은 페널티 없음
        _pendingNegativeGPPenalty = 0;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
        {
            { EProperties.GP.ToString(), PlayerGunpowder }
        });

        _initialized = false;
        return PlayerGunpowder;
    }

    /// <summary>
    /// 라운드 시작 시 음수 GP였던 만큼 HP에서 차감해야 하는 페널티 값을 한 번 소비하고 반환.
    /// PlayerStat.Start에서 SetPlayer 직후 호출하여 _currentHP에서 차감.
    /// </summary>
    public int ConsumePendingNegativeGPPenalty()
    {
        int penalty = _pendingNegativeGPPenalty;
        _pendingNegativeGPPenalty = 0;
        return penalty;
    }
}
