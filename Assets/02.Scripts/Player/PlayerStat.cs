using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    private PhotonView _photonView;
    [Header("Scriptable Object Reference")]
    [SerializeField] private PlayerStatSO _playerStatSO;
    public PlayerStatSO PlayerStatSO => _playerStatSO;

    [Header("Movement Stats")]
    [SerializeField] private float _moveSpeed;
    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }

    [SerializeField] private float _jumpForce;
    public float JumpForce { get => _jumpForce; set => _jumpForce = value; }

    [SerializeField] private float _dashTime;
    public float DashTime { get => _dashTime; set => _dashTime = value; }

    [SerializeField] private float _doubleTapTime;
    public float DoubleTapTime { get => _doubleTapTime; set => _doubleTapTime = value; }

    [SerializeField] private float _dashSpeed;
    public float DashSpeed { get => _dashSpeed; set => _dashSpeed = value; }

    [SerializeField] private float _breakTime;
    public float BreakTime { get => _breakTime; set => _breakTime = value; }

    [SerializeField] private float _runSpeed;
    public float RunSpeed { get => _runSpeed; set => _runSpeed = value; }

    [SerializeField] private float _maxJumpCount;
    public float MaxJumpCount { get => _maxJumpCount; set => _maxJumpCount = value; }

    [SerializeField] private float _jumpDashCount;
    public float JumpDashCount { get => _jumpDashCount; set => _jumpDashCount = value; }

    [SerializeField] private float _recoilTime;
    public float RecoilTime { get => _recoilTime; set => _recoilTime = value; }

    [SerializeField] private float _recoilSpeed;
    public float RecoilSpeed { get => _recoilSpeed; set => _recoilSpeed = value; }

    [SerializeField] private float _normalRecoilTime;
    public float NormalRecoilTime { get => _normalRecoilTime; set => _normalRecoilTime = value; }
    [SerializeField] private float _normalRecoilSpeed;
    public float NormalRecoilSpeed { get => _normalRecoilSpeed; set => _normalRecoilSpeed = value; }
    [SerializeField] private float _confuseTime = 5f;
    public float ConfuseTime { get => _confuseTime; set => _confuseTime = value; }
    [SerializeField] private float _hitStopGunPowderCount = 50;
    public float HitStopGunPowderCount { get => _hitStopGunPowderCount; set => _hitStopGunPowderCount = value; }
    private float _originalMoveSpeed;
    private float _originalRunSpeed;
    private float _originalJumpForce;




    [Header("Player State Stats")]
    [SerializeField] private bool _isRunning = false;
    public bool IsRunning { get => _isRunning; set => _isRunning = value; }

    [SerializeField] private bool _isJumping = false;
    public bool IsJumping { get => _isJumping; set => _isJumping = value; }
    [SerializeField] private bool _isFallingDead = false;
    public bool IsFallingDead { get => _isFallingDead; set => _isFallingDead = value; }
    [SerializeField] private bool _isImmune = false;
    public bool IsImmune { get => _isImmune; set => _isImmune = value; }
    [SerializeField] private bool _isDownJump = false;
    public bool IsDownJump { get => _isDownJump; set => _isDownJump = value; }
    [SerializeField] private bool _isWet = false;
    public bool IsWet { get => _isWet; set => _isWet = value; }

    [SerializeField] private float _myMoveSpeed;
    public float MyMoveSpeed { get => _myMoveSpeed; set => _myMoveSpeed = value; }

    [SerializeField] private float _jumpCount = 0;
    public float JumpCount { get => _jumpCount; set => _jumpCount = value; }

    [SerializeField] private int _facingDirection = 1;
    public int FacingDirection { get => _facingDirection; set => _facingDirection = value; }

    [Header("Player Attributes")]
    [SerializeField] private float _gunPowderDecreaseTime;
    public float GunPowderDecreaseTime { get => _gunPowderDecreaseTime; set => _gunPowderDecreaseTime = value; }
    [SerializeField] private int _attackPenaltyTime;
    public int AttackPenaltyTime { get => _attackPenaltyTime; set => _attackPenaltyTime = value; }

    [SerializeField] private int _attackPenaltyAmount;
    public int AttackPenaltyAmount { get => _attackPenaltyAmount; set => _attackPenaltyAmount = value; }
    [SerializeField] private int _initGunpowderCount;
    public int InitGunpowderCount => _initGunpowderCount;
    

    [Header("Die")]
    [SerializeField] private int _dieExplosionDamage;
    public int DieExplosionDamage { get => _dieExplosionDamage; set => _dieExplosionDamage = value; }
    [SerializeField] private float _dieExplosionRadius;
    public float DieExplosionRadius { get => _dieExplosionRadius; set => _dieExplosionRadius = value; }
    [SerializeField] private float _dieExplosionForce;
    public float DieExplosionForce { get => _dieExplosionForce; set => _dieExplosionForce = value; }
    [SerializeField] private float _invincibleTime;
    public float InvincibleTime { get => _invincibleTime; set => _invincibleTime = value; }

    [Header("Current Player")]
    // 현재 플레이어가 가지고 있는 수치
    [SerializeField] private int _currentPlayerGunPowderCount;
    public int CurrentPlayerGunPowderCount => _currentPlayerGunPowderCount;    
    
    [SerializeField] private int _currentPlayerLife;
    public int CurrentPlayerLife => _currentPlayerLife;
    [SerializeField] private int _currentPlayerDamagedCount;
    public int CurrentPlayerDamagedCount => _currentPlayerDamagedCount;

    [SerializeField]
    private List<SpriteRenderer> _mySpriteRendererList;
    public List<SpriteRenderer> MySpriteREndererList => _mySpriteRendererList;

    [Header("Damaged")]
    [SerializeField] private float _damagedTime;
    public float DamagedTime { get => _damagedTime; set => _damagedTime = value; }

    [Header("Damage, Death")]
    [SerializeField] private float _totalDamage;
    public float TotalDamage => _totalDamage;
    [SerializeField] private float _totalKillCount;
    public float TotalKillCount => _totalKillCount;
    
    [Header("Team")]
    public EInGameTeam Team { get; set; }
    public event Action OnGunPowderEmpty;
    
    
    public bool IsFallingFromLedge = false;

    [Header("Ultimate")]
    // 궁극기 사용 기회 관련 상태값
    [SerializeField] private bool _hasUltimateChance = false; // 궁극기 사용가능 상태
    public bool HasUltimateChance { get => _hasUltimateChance; set => _hasUltimateChance = value; }

    [SerializeField] private bool _hasUsedUltimateThisLife = false; // 현재 라이프에서 이미 궁극기를 사용했는지
    public bool HasUsedUltimateThisLife { get => _hasUsedUltimateThisLife; set => _hasUsedUltimateThisLife = value; }

    [SerializeField] private float _ultimateChanceDuration = 5f; // 기회가 지속되는 시간(초)
    public float UltimateChanceDuration { get => _ultimateChanceDuration; set => _ultimateChanceDuration = value; }

    [SerializeField] private int _ultimateTriggerThreshold = 30; // 건파우더(체력)가 이 값 이하가 되면 기회 발생 조건 충족
    public int UltimateTriggerThreshold { get => _ultimateTriggerThreshold; set => _ultimateTriggerThreshold = value; }


    void Start()
    {
        
        _photonView = GetComponent<PhotonView>();
        
        if (_photonView.IsMine == false)
        {
            return;
        }
        InitializeStats(); 
        SetPlayer(RoomStatManager.Instance.PlayerGunpowder,RoomStatManager.Instance.PlayerLife,RoomStatManager.Instance.PlayerDecreaseTime, RoomStatManager.Instance.PlayerTeam);
        
        Debug.Log($"{Team.ToString()}");
    }

    public void InitializeStats()
    {
        if (_playerStatSO != null)
        {
            _moveSpeed = _playerStatSO.MoveSpeed;
            _jumpForce = _playerStatSO.JumpForce;
            _dashTime = _playerStatSO.DashTime;
            _doubleTapTime = _playerStatSO.DoubleTapTime;
            _dashSpeed = _playerStatSO.DashSpeed;
            _breakTime = _playerStatSO.BreakTime;
            _runSpeed = _playerStatSO.RunSpeed;
            _maxJumpCount = _playerStatSO.MaxJumpCount;
            _recoilTime = _playerStatSO.RecoilTime;
            _recoilSpeed = _playerStatSO.RecoilSpeed;
            _normalRecoilTime = _playerStatSO.NormalRecoilTime;
            _normalRecoilSpeed = _playerStatSO.NormalRecoilSpeed;
            _attackPenaltyTime = _playerStatSO.AttackPenaltyTime;
            _attackPenaltyAmount = _playerStatSO.AttackPenaltyAmount;
            // _gunPowderDecreaseTime = _playerStatSO.GunPowderDecreaseTime;
            _dieExplosionDamage = _playerStatSO.DieExplosionDamage;
            _dieExplosionRadius = _playerStatSO.DieExplosionRadius;
            _dieExplosionForce = _playerStatSO.DieExplosionForce;
            _invincibleTime = _playerStatSO.InvincibleTime;
            _damagedTime = _playerStatSO.DamagedTime;

            _originalMoveSpeed = _moveSpeed;
            _originalRunSpeed = _runSpeed;
            _originalJumpForce = _jumpForce;

            // 나중에는 방 설정에 따라 달라질 수 있음.
            _currentPlayerGunPowderCount = RoomStatManager.Instance.PlayerGunpowder;
            _initGunpowderCount = RoomStatManager.Instance.PlayerGunpowder;
            _currentPlayerLife = RoomStatManager.Instance.PlayerLife;
            _gunPowderDecreaseTime = RoomStatManager.Instance.PlayerDecreaseTime;

            
            _currentPlayerDamagedCount = 0;
        }
    } 

    public void SetPlayer(int gunpowder, int life, int decrease, EInGameTeam team)
    {
        _currentPlayerGunPowderCount = gunpowder;
        _currentPlayerLife = life;
        _initGunpowderCount = gunpowder;
        _gunPowderDecreaseTime = decrease;
        Team = team;
    }

    public void SetPlayerGunPowderCountAndLife(int gunpowder, int life)
    {
        _currentPlayerGunPowderCount = gunpowder;
        _currentPlayerLife = life;
    }

    public void ResetJumpCount()
    {
        _jumpCount = 0;
    }

    public void IncrementJumpCount()
    {
        _jumpCount++;
    }

    public bool CanJump()
    {
        return _jumpCount < _maxJumpCount;
    }

    public void IncrementJumpDashCount()
    {
        _jumpDashCount++;
    }

    public bool CanJumpDash()
    {
        return _jumpDashCount < 1;
    }

    public void ResetJumpDashCount()
    {
        _jumpDashCount = 0;
    }
    public void IncreaseGunPowderCount(int amount)
    {
        if(!_photonView.IsMine)
        {
            return;
        }
        
        _currentPlayerGunPowderCount += amount;
        _photonView.RPC(nameof(RPC_ChangeGunpowder), RpcTarget.All, _currentPlayerGunPowderCount,
            _currentPlayerLife, 0);
    }

    [PunRPC]
    public void RPC_RequestIncreaseGunPowder(int amount, PhotonMessageInfo info)
    {
        if (GameManager.Instance.CurrentGameState != EGameState.Playing)
        {
            return;
        } 
        
        // 소유자만 처리 (마스터가 보내더라도 최종 처리는 로컬 소유자 권한)
        if (!_photonView.IsMine)
        {
            return;
        }
        _currentPlayerGunPowderCount += amount;
        _photonView.RPC(nameof(RPC_ChangeGunpowder), RpcTarget.All, _currentPlayerGunPowderCount,
            _currentPlayerLife, 0);
    }
    
    public bool DecreaseGunPowderCount(int amount, int attacker)
    {
        if (GameManager.Instance.CurrentGameState != EGameState.Playing)
        {
            return false;
        } 
        if(!_photonView.IsMine)
        {
            return false;
        }

        if (_isImmune)
        {
            return false;
        }
        
        _currentPlayerGunPowderCount -= amount;
        bool isDead = false;

        // 피가 30이하가 되면 궁극기 사용기회 주어짐
        // 라이프당 1번만 기회가 주어짐
        if(_currentPlayerGunPowderCount <= _ultimateTriggerThreshold && !_hasUltimateChance && !_hasUsedUltimateThisLife)
        {
            _hasUltimateChance = true;
        }

        if (_currentPlayerGunPowderCount <= 0)
        {
            _currentPlayerLife -= 1;
            _currentPlayerGunPowderCount = _initGunpowderCount;
            isDead = true;
            OnGunPowderEmpty?.Invoke();
        }
        
        if(_currentPlayerLife <= 0)
        {
            _currentPlayerGunPowderCount = 0;
        }
        
        _photonView.RPC(nameof(RPC_ChangeGunpowder), RpcTarget.All, _currentPlayerGunPowderCount,
            _currentPlayerLife, attacker);

        return isDead;
    }

    [PunRPC]
    private void RPC_ChangeGunpowder(int gunpowder, int life, int attacker, PhotonMessageInfo info)
    {
        DamageChecker.Instance.RPC_RequestDamage(gunpowder, life , info.Sender.ActorNumber, attacker);
    }

    public void IncreseDamagedCount()
    {
        _currentPlayerDamagedCount++;
    }

    public void ResetDamagedCount()
    {
        _currentPlayerDamagedCount = 0;
    }

    public void IncreaseTotalDamage(float damage)
    {
        _totalDamage += damage;
    }
    
    public void ResetTotalDamage()
    {
        _totalDamage = 0;
    }
    
    public void IncreaseTotalKillCount()
    {
        _totalKillCount++;
        Debug.Log(_totalKillCount);
    }

    public void ResetTotalKillCount()
    {
        _totalKillCount = 0;
    }

    public void ResurrectPlayerStat()
    {
        if(!_photonView.IsMine)
        {
            return;
        }

        Debug.Log("플레이어 부활");
        
        // 건파우더 초기화
        _currentPlayerGunPowderCount = _initGunpowderCount;
        
        // 플레이어 상태 초기화
        _isRunning = false;
        _isJumping = false;
        _myMoveSpeed = _moveSpeed;
        _jumpCount = 0;
        _facingDirection = 1;
        _isFallingDead = false;
        _hasUsedUltimateThisLife = false;
        _hasUltimateChance = false;
        
        // 네트워크 동기화
        _photonView.RPC(nameof(RPC_ChangeGunpowder), RpcTarget.All, _currentPlayerGunPowderCount, _currentPlayerLife, 0);
        
        Debug.Log($"[PlayerStat] ResurrectPlayerStat - Life: {_currentPlayerLife}, GunPowder: {_currentPlayerGunPowderCount}");
    }

    public void SetConfuseTime(float time)
    {
        _confuseTime = time;
    }

    public void SetMoveSpeed(float speed)
    {
        _moveSpeed = speed;
    }

    public void SetRunSpeed(float speed)
    {
        _runSpeed = speed;
    }

    public void SetJumpForce(float force)
    {
        _jumpForce = force;
    }

    public void ResetMoveSpeed()
    {
        _moveSpeed = _originalMoveSpeed;
    }

    public void ResetRunSpeed()
    {
        _runSpeed = _originalRunSpeed;
    }

    public void ResetJumpForce()
    {
        _jumpForce = _originalJumpForce;
    }

    public void SetWetState()
    {
        SetMoveSpeed(_originalMoveSpeed * 0.5f);
        SetRunSpeed(_originalRunSpeed * 0.5f);
        SetJumpForce(_originalJumpForce * 0.5f);
        _isWet = true;
    }

    public void ResetWetState()
    {
        ResetMoveSpeed();
        ResetRunSpeed();
        ResetJumpForce();
        _isWet = false;
    }
}
