using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
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



    [Header("Player State Stats")]
    [SerializeField] private bool _isRunning = false;
    public bool IsRunning { get => _isRunning; set => _isRunning = value; }

    [SerializeField] private bool _isJumping = false;
    public bool IsJumping { get => _isJumping; set => _isJumping = value; }
    [SerializeField] private bool _isFallingDead = false;
    public bool IsFallingDead { get => _isFallingDead; set => _isFallingDead = value; }

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
    [SerializeField] private int _currentPlayerDamagedCount;

    public int CurrentPlayerLife => _currentPlayerLife;
    [SerializeField] private int _currentPlayerLife;
    [SerializeField] private int _initGunpowderCount;
    public int CurrentPlayerDamagedCount => _currentPlayerDamagedCount;

    [SerializeField]
    private List<SpriteRenderer> _mySpriteRendererList;
    public List<SpriteRenderer> MySpriteREndererList => _mySpriteRendererList;

    [Header("Damaged")]
    [SerializeField] private float _damagedTime;
    public float DamagedTime { get => _damagedTime; set => _damagedTime = value; }

    
    public event Action OnGunPowderEmpty;
    
    
    public bool IsFallingFromLedge = false;

    void OnEnable()
    {
        InitializeStats();
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
            _gunPowderDecreaseTime = _playerStatSO.GunPowderDecreaseTime;
            _dieExplosionDamage = _playerStatSO.DieExplosionDamage;
            _dieExplosionRadius = _playerStatSO.DieExplosionRadius;
            _dieExplosionForce = _playerStatSO.DieExplosionForce;
            _invincibleTime = _playerStatSO.InvincibleTime;
            _damagedTime = _playerStatSO.DamagedTime;

            // 나중에는 방 설정에 따라 달라질 수 있음.
            // _currentPlayerGunPowderCount = _playerStatSO.MaxGunPoderCount;
            _currentPlayerDamagedCount = 0;
        }
    } 

    public void SetPlayer(int gunpowder, int life)
    {
        _currentPlayerGunPowderCount = gunpowder;
        _currentPlayerLife = life;
        _initGunpowderCount = gunpowder;
        
        Debug.Log("SetPlayer");
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
        _currentPlayerGunPowderCount += amount;
    }

    public void DecreaseGunPowderCount(int amount)
    {
        _currentPlayerGunPowderCount -= amount;


        if (_currentPlayerGunPowderCount <= 0)
        {
            _currentPlayerLife -= 1;
            _currentPlayerGunPowderCount = _initGunpowderCount;
        }
        
        if(_currentPlayerLife <= 0)
        {
            _currentPlayerGunPowderCount = 0;
            OnGunPowderEmpty?.Invoke();
        }
    }

    public void IncreseDamagedCount()
    {
        _currentPlayerDamagedCount++;
    }

    public void ResetDamagedCount()
    {
        _currentPlayerDamagedCount = 0;
    }
    
}
