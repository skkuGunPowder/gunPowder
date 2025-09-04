using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    // 내부 필드
    private PhotonView _photonView;
    private float _originalMoveSpeed;
    private float _originalRunSpeed;
    private float _originalJumpForce;

    [Header("스크립터블 오브젝트 참조")]
    [SerializeField] private PlayerStatSO _playerStatSO;
    public PlayerStatSO PlayerStatSO => _playerStatSO;

    [Header("이동 설정")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _maxJumpCount;
    
    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
    public float RunSpeed { get => _runSpeed; set => _runSpeed = value; }
    public float JumpForce { get => _jumpForce; set => _jumpForce = value; }
    public float MaxJumpCount { get => _maxJumpCount; set => _maxJumpCount = value; }

    [Header("대시 설정")]
    [SerializeField] private float _dashTime;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _doubleTapTime;
    [SerializeField] private float _breakTime;
    [SerializeField] private float _jumpDashCount;
    
    public float DashTime { get => _dashTime; set => _dashTime = value; }
    public float DashSpeed { get => _dashSpeed; set => _dashSpeed = value; }
    public float DoubleTapTime { get => _doubleTapTime; set => _doubleTapTime = value; }
    public float BreakTime { get => _breakTime; set => _breakTime = value; }
    public float JumpDashCount { get => _jumpDashCount; set => _jumpDashCount = value; }

    [Header("반동 설정")]
    [SerializeField] private float _recoilTime;
    [SerializeField] private float _recoilSpeed;
    [SerializeField] private float _normalRecoilTime;
    [SerializeField] private float _normalRecoilSpeed;
    
    public float RecoilTime { get => _recoilTime; set => _recoilTime = value; }
    public float RecoilSpeed { get => _recoilSpeed; set => _recoilSpeed = value; }
    public float NormalRecoilTime { get => _normalRecoilTime; set => _normalRecoilTime = value; }
    public float NormalRecoilSpeed { get => _normalRecoilSpeed; set => _normalRecoilSpeed = value; }

    [Header("상태 효과")]
    [SerializeField] private float _confuseTime = 5f;
    [SerializeField] private float _hitStopGunPowderCount = 50;
    [SerializeField] private float _damagedTime;
    [SerializeField] private float _invincibleTime;
    
    public float ConfuseTime { get => _confuseTime; set => _confuseTime = value; }
    public float HitStopGunPowderCount { get => _hitStopGunPowderCount; set => _hitStopGunPowderCount = value; }
    public float DamagedTime { get => _damagedTime; set => _damagedTime = value; }
    public float InvincibleTime { get => _invincibleTime; set => _invincibleTime = value; }

    [Header("플레이어 상태")]
    [SerializeField] private bool _isRunning = false;
    [SerializeField] private bool _isJumping = false;
    [SerializeField] private bool _isFallingDead = false;
    [SerializeField] private bool _isImmune = false;
    [SerializeField] private bool _isDownJump = false;
    [SerializeField] private bool _isWet = false;
    [SerializeField] private float _myMoveSpeed;
    [SerializeField] private float _jumpCount = 0;
    [SerializeField] private int _facingDirection = 1;
    
    public bool IsRunning { get => _isRunning; set => _isRunning = value; }
    public bool IsJumping { get => _isJumping; set => _isJumping = value; }
    public bool IsFallingDead { get => _isFallingDead; set => _isFallingDead = value; }
    public bool IsImmune { get => _isImmune; set => _isImmune = value; }
    public bool IsDownJump { get => _isDownJump; set => _isDownJump = value; }
    public bool IsWet { get => _isWet; set => _isWet = value; }
    public float MyMoveSpeed { get => _myMoveSpeed; set => _myMoveSpeed = value; }
    public float JumpCount { get => _jumpCount; set => _jumpCount = value; }
    public int FacingDirection { get => _facingDirection; set => _facingDirection = value; }
    public bool IsFallingFromLedge = false;

    [Header("플레이어 리소스")]
    [SerializeField] private int _currentPlayerGunPowderCount;
    [SerializeField] private int _currentPlayerLife;
    [SerializeField] private int _initGunpowderCount;
    [SerializeField] private float _gunPowderDecreaseTime;
    [SerializeField] private int _currentPlayerDamagedCount;
    
    public int CurrentPlayerGunPowderCount => _currentPlayerGunPowderCount;
    public int CurrentPlayerLife => _currentPlayerLife;
    public int InitGunpowderCount => _initGunpowderCount;
    public float GunPowderDecreaseTime { get => _gunPowderDecreaseTime; set => _gunPowderDecreaseTime = value; }
    public int CurrentPlayerDamagedCount => _currentPlayerDamagedCount;

    [Header("공격 설정")]
    [SerializeField] private int _attackPenaltyTime;
    [SerializeField] private int _attackPenaltyAmount;
    
    public int AttackPenaltyTime { get => _attackPenaltyTime; set => _attackPenaltyTime = value; }
    public int AttackPenaltyAmount { get => _attackPenaltyAmount; set => _attackPenaltyAmount = value; }

    [Header("사망 설정")]
    [SerializeField] private int _dieExplosionDamage;
    [SerializeField] private float _dieExplosionRadius;
    [SerializeField] private float _dieExplosionForce;
    
    public int DieExplosionDamage { get => _dieExplosionDamage; set => _dieExplosionDamage = value; }
    public float DieExplosionRadius { get => _dieExplosionRadius; set => _dieExplosionRadius = value; }
    public float DieExplosionForce { get => _dieExplosionForce; set => _dieExplosionForce = value; }

    [Header("통계")]
    [SerializeField] private float _totalDamage;
    [SerializeField] private float _totalKillCount;
    
    public float TotalDamage => _totalDamage;
    public float TotalKillCount => _totalKillCount;

    [Header("궁극기 시스템")]
    [SerializeField] private bool _hasUltimateChance = false; // 궁극기 사용가능 상태
    [SerializeField] private bool _hasUsedUltimateThisLife = false; // 현재 라이프에서 이미 궁극기를 사용했는지
    [SerializeField] private float _ultimateChanceDuration = 10f; // 기회가 지속되는 시간(초)
    [SerializeField] private int _ultimateTriggerThreshold = 30; // 건파우더(체력)가 이 값 이하가 되면 기회 발생 조건 충족
    
    public bool HasUltimateChance { get => _hasUltimateChance; set => _hasUltimateChance = value; }
    public bool HasUsedUltimateThisLife { get => _hasUsedUltimateThisLife; set => _hasUsedUltimateThisLife = value; }
    public float UltimateChanceDuration { get => _ultimateChanceDuration; set => _ultimateChanceDuration = value; }
    public int UltimateTriggerThreshold { get => _ultimateTriggerThreshold; set => _ultimateTriggerThreshold = value; }

    [Header("팀 & 이벤트")]
    [SerializeField] private List<SpriteRenderer> _mySpriteRendererList;
    
    public EInGameTeam Team { get; set; }
    public List<SpriteRenderer> MySpriteREndererList => _mySpriteRendererList;
    public PlayerSFXAnimationEvent PlayerSFXAnimationEvent;
    public event Action OnGunPowderEmpty;
    public event Action<int> OnGunpowderIncreased;

    void Start()
    {
        _photonView = GetComponent<PhotonView>();
        
        if (_photonView.IsMine == false)
        {
            return;
        }
        
        //InitializeStats(); 
        SetPlayer(RoomStatManager.Instance.PlayerGunpowder, RoomStatManager.Instance.PlayerLife, 
                 RoomStatManager.Instance.PlayerDecreaseTime, RoomStatManager.Instance.PlayerTeam);
    }

    // 초기화 관련 메서드
    public void InitializeStats()
    {
        if (_playerStatSO != null)
        {
            // 이동 스탯
            _moveSpeed = _playerStatSO.MoveSpeed;
            _runSpeed = _playerStatSO.RunSpeed;
            _jumpForce = _playerStatSO.JumpForce;
            _maxJumpCount = _playerStatSO.MaxJumpCount;
            
            // 대시 스탯
            _dashTime = _playerStatSO.DashTime;
            _dashSpeed = _playerStatSO.DashSpeed;
            _doubleTapTime = _playerStatSO.DoubleTapTime;
            _breakTime = _playerStatSO.BreakTime;
            
            // 반동 스탯
            _recoilTime = _playerStatSO.RecoilTime;
            _recoilSpeed = _playerStatSO.RecoilSpeed;
            _normalRecoilTime = _playerStatSO.NormalRecoilTime;
            _normalRecoilSpeed = _playerStatSO.NormalRecoilSpeed;
            
            // 공격 스탯
            _attackPenaltyTime = _playerStatSO.AttackPenaltyTime;
            _attackPenaltyAmount = _playerStatSO.AttackPenaltyAmount;
            
            // 사망 스탯
            _dieExplosionDamage = _playerStatSO.DieExplosionDamage;
            _dieExplosionRadius = _playerStatSO.DieExplosionRadius;
            _dieExplosionForce = _playerStatSO.DieExplosionForce;
            
            // 상태 효과
            _invincibleTime = _playerStatSO.InvincibleTime;
            _damagedTime = _playerStatSO.DamagedTime;

            // 원본 값 저장 (리셋용)
            _originalMoveSpeed = _moveSpeed;
            _originalRunSpeed = _runSpeed;
            _originalJumpForce = _jumpForce;

            // 방 설정에서 플레이어 리소스 초기화
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

    // 점프 관리 메서드
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

    // 건파우더 관리 메서드
    public void IncreaseGunPowderCount(int amount)
    {
        if (!_photonView.IsMine)
        {
            return;
        }
        
        _currentPlayerGunPowderCount += amount;
        _photonView.RPC(nameof(RPC_ChangeGunpowder), RpcTarget.All, _currentPlayerGunPowderCount,
            _currentPlayerLife, 0);
        OnGunpowderIncreased?.Invoke(amount);
    }

    [PunRPC]
    public void RPC_RequestIncreaseGunPowder(int amount, PhotonMessageInfo info)
    {
        if (GameManager.Instance.CurrentGameState != EGameState.Playing)
        {
            return;
        }

        if (!_photonView.IsMine)
        {
            return;
        }
        
        _currentPlayerGunPowderCount += amount;
        _photonView.RPC(nameof(RPC_ChangeGunpowder), RpcTarget.All, _currentPlayerGunPowderCount,
            _currentPlayerLife, 0);
        OnGunpowderIncreased?.Invoke(amount);
    }
    
    public bool DecreaseGunPowderCount(int amount, int attacker)
    {
        if (GameManager.Instance.CurrentGameState != EGameState.Playing)
        {
            return false;
        }
        
        if (!_photonView.IsMine)
        {
            return false;
        }

        if (_isImmune)
        {
            return false;
        }
        
        _currentPlayerGunPowderCount -= amount;
        bool isDead = false;

        // 궁극기 기회 발생 조건 확인
        if (_currentPlayerGunPowderCount <= _ultimateTriggerThreshold && 
            !_hasUltimateChance && !_hasUsedUltimateThisLife)
        {
            _hasUltimateChance = true;
        }

        if (_currentPlayerGunPowderCount <= 0)
        {
            _currentPlayerLife -= 1;
            _currentPlayerGunPowderCount = _initGunpowderCount;
            isDead = true;
            _photonView.RPC(nameof(RPC_Dead), RpcTarget.All, attacker);
            OnGunPowderEmpty?.Invoke();
        }
        
        if (_currentPlayerLife <= 0)
        {
            _currentPlayerGunPowderCount = 0;
        }
        
        _photonView.RPC(nameof(RPC_ChangeGunpowder), RpcTarget.All, _currentPlayerGunPowderCount,
            _currentPlayerLife, attacker);

        return isDead;
    }

    [PunRPC]
    private void RPC_ChangeGunpowder(int gunpowder, int life, int attacker ,PhotonMessageInfo info)
    {
        DamageChecker.Instance.RPC_RequestDamage(gunpowder, life, attacker,info.Sender.ActorNumber);
    }

    [PunRPC]
    private void RPC_Dead(int attacker, PhotonMessageInfo info)
    {
        DamageChecker.Instance.ActiveKillLog(attacker, info.Sender.ActorNumber);
    }

    // 데미지 & 통계 관리 메서드
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
    }

    public void ResetTotalKillCount()
    {
        _totalKillCount = 0;
    }

    // 플레이어 상태 관리 메서드
    public void ResurrectPlayerStat()
    {
        if (!_photonView.IsMine)
        {
            return;
        }
        
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
    }

    // 상태 효과 관리 메서드
    public void SetConfuseTime(float time)
    {
        _confuseTime = time;
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

    // 이동 능력치 수정자 메서드
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
}
