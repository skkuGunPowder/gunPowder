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
    [SerializeField] private bool _isPausedNoAttack = false;
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
    public bool IsPausedNoAttack { get => _isPausedNoAttack; set => _isPausedNoAttack = value; }
    public bool IsFallingFromLedge = false;

    [Header("HP (체력)")]
    [SerializeField] private int _currentHP;
    [SerializeField] private int _currentPlayerLife;
    private const int INIT_HP = 150;
    [SerializeField] private float _gunPowderDecreaseTime;
    public int CurrentHP => _currentHP;
    public int CurrentPlayerLife => _currentPlayerLife;
    public int InitHP => INIT_HP;
    // 하위 호환성 프로퍼티 (기존 코드에서 참조하는 곳을 위해 유지)
    public int CurrentPlayerGunPowderCount => _currentHP;
    public int InitGunpowderCount => INIT_HP;
    public float GunPowderDecreaseTime { get => _gunPowderDecreaseTime; set => _gunPowderDecreaseTime = value; }

    [Header("GP (재화)")]
    [SerializeField] private int _currentGP;
    public int CurrentGP => _currentGP;
    public event Action<int> OnGPChanged;

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

    [Header("궁극기 게이지 시스템")]
    [SerializeField] private float _currentUltimateGauge = 0f;
    [SerializeField] private float _maxUltimateGauge = 100f;
    private const float ULTIMATE_GAUGE_RATE_ON_DAMAGE_DEALT = 0.3f;  // 주는 데미지의 30%
    private const float ULTIMATE_GAUGE_RATE_ON_DAMAGE_TAKEN = 0.5f;  // 받는 데미지의 50%

    public float CurrentUltimateGauge => _currentUltimateGauge;
    public float MaxUltimateGauge => _maxUltimateGauge;
    public bool IsUltimateReady => _currentUltimateGauge >= _maxUltimateGauge;
    public event Action<float, float> OnUltimateGaugeChanged;

    [Header("최근 공격자 추적")]
    [SerializeField] private int _lastAttackerActorNumber = -1;
    [SerializeField] private float _lastAttackTime = 0f;
    [SerializeField] private float _attackTrackingDuration = 10f;
    private bool _lastIsNormalAttack = true;

    public int LastAttackerActorNumber => _lastAttackerActorNumber;
    public float LastAttackTime => _lastAttackTime;
    public float AttackTrackingDuration => _attackTrackingDuration;
    public bool LastIsNormalAttack => _lastIsNormalAttack;

    [Header("팀 & 이벤트")]
    [SerializeField] private List<SpriteRenderer> _mySpriteRendererList;

    public EInGameTeam Team { get; set; }
    public List<SpriteRenderer> MySpriteREndererList => _mySpriteRendererList;
    public PlayerSFXAnimationEvent PlayerSFXAnimationEvent;
    public event Action OnHPEmpty;
    public event Action<int> OnHPIncreased;
    public event Action<int> OnHPChanged;

    void Start()
    {
        _photonView = GetComponent<PhotonView>();

        if (_photonView.IsMine == false)
        {
            return;
        }

        SetPlayer(RoomStatManager.Instance.GetGunpowder(), RoomStatManager.Instance.PlayerLife,
                 RoomStatManager.Instance.PlayerDecreaseTime);

        // 라운드 시작 시 GP가 음수였다면 빚 만큼 HP에서 차감 (RoomStatManager.GetGunpowder가 캐시한 값)
        ApplyNegativeGPHPPenalty();

        // 라운드 전환으로 Player GO가 재생성되어도 궁극기 게이지가 유지되도록
        // Photon LocalPlayer CustomProperties에서 복원
        RestoreUltimateGaugeFromCustomProperty();

        // 라운드 종료(=GameOver 이벤트) 시점에 게이지를 영속화 (PhotonNetwork.DestroyAll 직전)
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGameOver -= SaveUltimateGaugeToCustomProperty;
            EventManager.Instance.OnGameOver += SaveUltimateGaugeToCustomProperty;
        }
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGameOver -= SaveUltimateGaugeToCustomProperty;
        }

        // OnGameOver를 못 받고 파괴되는 경우 대비한 마지막 저장 (소유자만)
        SaveUltimateGaugeToCustomProperty();
    }

    /// <summary>
    /// 라운드 시작 시 GP가 음수였던 만큼 HP에서 차감. RoomStatManager가 캐시한 페널티 값을 1회 소비.
    /// HP는 최소 1로 보장.
    /// </summary>
    private void ApplyNegativeGPHPPenalty()
    {
        if (RoomStatManager.Instance == null) return;

        int penalty = RoomStatManager.Instance.ConsumePendingNegativeGPPenalty();
        if (penalty <= 0) return;

        _currentHP = Mathf.Max(1, _currentHP - penalty);

        if (_photonView != null)
        {
            _photonView.RPC(nameof(RPC_ChangeHP), RpcTarget.All, _currentHP, _currentPlayerLife, 0);
        }
        OnHPChanged?.Invoke(_currentHP);

        // DamageChecker는 GameState != Playing이면 OnDataChanged를 발화하지 않음.
        // 또한 UI_InGameProfile.Init이 PlayerStat.Start보다 늦게 실행되면 _playerActorNumberList가
        // 비어 있어 Refresh가 no-op이 됨. 다음 프레임에 직접 PlayerDataChange를 호출해 둘 다 우회.
        StartCoroutine(FirePlayerDataChangedNextFrame(_currentHP, _currentPlayerLife));
    }

    private System.Collections.IEnumerator FirePlayerDataChangedNextFrame(int hp, int life)
    {
        yield return null; // UI Init / 등록 완료 대기
        if (EventManager.Instance == null || _photonView == null) yield break;
        EventManager.Instance.PlayerDataChange(hp, life, _photonView.OwnerActorNr, 0);
    }

    private void SaveUltimateGaugeToCustomProperty()
    {
        if (_photonView == null || !_photonView.IsMine) return;
        if (PhotonNetwork.LocalPlayer == null) return;

        // Playing(=실제 라운드 진행 중)이 아니면 저장하지 않음
        // (Result/매치 종료 시점에 저장되어 다음 매치로 넘어가는 것을 방지)
        if (GameManager.Instance == null
            || GameManager.Instance.CurrentGameState != EGameState.Playing)
        {
            return;
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { EProperties.UltimateGauge.ToString(), _currentUltimateGauge }
        });
    }

    private void RestoreUltimateGaugeFromCustomProperty()
    {
        if (_photonView == null || !_photonView.IsMine) return;
        if (PhotonNetwork.LocalPlayer == null) return;

        // Playing이 아닌 상태(Result/Waiting 등)에서는 복원하지 않고, 남아있는 값은 정리
        if (GameManager.Instance == null
            || GameManager.Instance.CurrentGameState != EGameState.Playing)
        {
            ClearUltimateGaugeCustomProperty();
            return;
        }

        if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(
                EProperties.UltimateGauge.ToString(), out object value))
        {
            return;
        }

        float saved = Convert.ToSingle(value);
        _currentUltimateGauge = Mathf.Clamp(saved, 0f, _maxUltimateGauge);
        OnUltimateGaugeChanged?.Invoke(_currentUltimateGauge, _maxUltimateGauge);
    }

    private void ClearUltimateGaugeCustomProperty()
    {
        if (PhotonNetwork.LocalPlayer == null) return;
        string key = EProperties.UltimateGauge.ToString();
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(key)) return;

        // Photon: 값을 null로 설정하면 키 자체가 삭제됨
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { key, null }
        });
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

            // HP 고정 150, GP는 로비 설정값
            _currentHP = INIT_HP;
            _currentPlayerLife = RoomStatManager.Instance.PlayerLife;
            _currentGP = RoomStatManager.Instance.PlayerGunpowder;
            _gunPowderDecreaseTime = RoomStatManager.Instance.PlayerDecreaseTime;
        }
    }

    public void SetPlayer(int gunpowder, int life, int decrease)
    {
        _currentHP = INIT_HP;
        _currentPlayerLife = life;
        _currentGP = gunpowder;
        _gunPowderDecreaseTime = decrease;
    }

    public void SetPlayerHPAndLife(int hp, int life)
    {
        _currentHP = hp;
        _currentPlayerLife = life;
    }

    // 하위 호환성
    public void SetPlayerGunPowderCountAndLife(int gunpowder, int life)
    {
        SetPlayerHPAndLife(gunpowder, life);
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

    // ===== HP 관리 메서드 =====

    public void IncreaseHP(int amount)
    {
        if (GameManager.Instance.CurrentGameState != EGameState.Playing &&
            GameManager.Instance.CurrentGameState != EGameState.Tutorial)
        {
            return;
        }

        if (!_photonView.IsMine)
        {
            return;
        }

        if(_currentHP <= 0)
        {
            return;
        }

        _currentHP += amount;

        _photonView.RPC(nameof(RPC_ChangeHP), RpcTarget.All, _currentHP,
            _currentPlayerLife, 0);
        OnHPIncreased?.Invoke(amount);
        OnHPChanged?.Invoke(_currentHP);
    }

    // 하위 호환성
    public void IncreaseGunPowderCount(int amount)
    {
        IncreaseHP(amount);
    }

    // ===== GP 관리 메서드 =====

    public void DecreaseGP(int amount)
    {
        if (!_photonView.IsMine) return;

        _currentGP -= amount; // 음수 허용
        _photonView.RPC(nameof(RPC_ChangeGP), RpcTarget.All, _currentGP);
        OnGPChanged?.Invoke(_currentGP);
        PlayerEventManager.Instance.GetEvents(_photonView.OwnerActorNr).InvokeOnGPLost(amount);
    }

    public void IncreaseGP(int amount)
    {
        if (!_photonView.IsMine) return;

        _currentGP += amount;
        _photonView.RPC(nameof(RPC_ChangeGP), RpcTarget.All, _currentGP);
        OnGPChanged?.Invoke(_currentGP);
        PlayerEventManager.Instance.GetEvents(_photonView.OwnerActorNr).InvokeOnGPGained(amount);
    }

    [PunRPC]
    public void RPC_ChangeGP(int gp, PhotonMessageInfo info)
    {
        _currentGP = gp;
        OnGPChanged?.Invoke(_currentGP);
        PlayerEventManager.Instance.GetEvents(_photonView.OwnerActorNr).InvokeOnGPSet(_currentGP);
        if (EventManager.Instance != null)
        {
            EventManager.Instance.PlayerGPChange(info.Sender.ActorNumber, _currentGP);
        }
    }

    [PunRPC]
    public void RPC_RequestIncreaseGP(int amount, PhotonMessageInfo info)
    {
        if (GameManager.Instance.CurrentGameState != EGameState.Playing)
        {
            return;
        }

        if (!_photonView.IsMine)
        {
            return;
        }

        _currentGP += amount;
        _photonView.RPC(nameof(RPC_ChangeGP), RpcTarget.All, _currentGP);
        OnGPChanged?.Invoke(_currentGP);
    }

    // 하위 호환성 (GunPowderBezierCurve에서 호출)
    [PunRPC]
    public void RPC_RequestIncreaseGunPowder(int amount, PhotonMessageInfo info)
    {
        RPC_RequestIncreaseGP(amount, info);
    }

    public bool DecreaseHP(int amount, int attacker, bool isNormalAttack = true, bool ignoreImmune = false)
    {
        if (GameManager.Instance.CurrentGameState != EGameState.Playing)
        {
            return false;
        }

        if (!_photonView.IsMine)
        {
            return false;
        }

        if (_isImmune && !ignoreImmune)
        {
            return false;
        }

        _currentHP -= amount;
        bool isDead = false;

        // 공격자 기록
        RecordLastAttacker(attacker);
        _lastIsNormalAttack = isNormalAttack;

        // 받는 데미지에 의한 피격자 궁극기 게이지 충전 (데미지의 50%)
        ChargeUltimateOnDamageTaken(amount);

        // 주는 데미지에 의한 공격자 궁극기 게이지 충전 (데미지의 30%)
        if (attacker != _photonView.OwnerActorNr && attacker > 0)
        {
            PhotonView attackerView = FindAttackerPhotonView(attacker);
            if (attackerView != null && attackerView.Owner != null)
            {
                attackerView.RPC(nameof(RPC_ChargeUltimateOnDamageDealt), attackerView.Owner, amount);
            }
        }

        if (_currentHP <= 0)
        {
            _currentPlayerLife -= 1;
            isDead = true;

            int killerForLog = GetValidLastAttacker();
            if (killerForLog == -1)
            {
                killerForLog = _photonView.OwnerActorNr; // 자살
            }

            _photonView.RPC(nameof(RPC_Dead), RpcTarget.All, killerForLog, isNormalAttack);
            OnHPEmpty?.Invoke();

            // 킬 카운트 증가 로직
            if (isDead)
            {
                int validLastAttacker = GetValidLastAttacker();

                if (validLastAttacker != -1)
                {
                    PhotonView attackerView = FindAttackerPhotonView(validLastAttacker);

                    if (attackerView != null && attackerView.Owner != null)
                    {
                        attackerView.RPC(nameof(RPC_IncreaseTotalKillCount), attackerView.Owner,
                            _photonView.OwnerActorNr, _currentPlayerLife <= 0, isNormalAttack);
                    }
                }
            }

            // 리스폰 캐리오버 로직
            if (_currentPlayerLife > 0)
            {
                ApplyResurrectCarryover();
            }
        }

        if (_currentPlayerLife <= 0)
        {
            _currentHP = 0;
        }

        _photonView.RPC(nameof(RPC_ChangeHP), RpcTarget.All, _currentHP,
            _currentPlayerLife, attacker);

        OnHPChanged?.Invoke(_currentHP);

        return isDead;
    }

    // 하위 호환성
    public bool DecreaseGunPowderCount(int amount, int attacker, bool isNormalAttack = true, bool ignoreImmune = false)
    {
        return DecreaseHP(amount, attacker, isNormalAttack, ignoreImmune);
    }

    /// <summary>
    /// 리스폰 캐리오버: GP에 따라 HP/GP를 조정
    /// GP >= 0: GP = GP + 50
    /// GP < 0: HP = 150 - min(|GP|, 100), GP = 50
    /// </summary>
    private void ApplyResurrectCarryover()
    {
        if (_currentGP >= 0)
        {
            _currentHP = INIT_HP;
            _currentGP += 50;
        }
        else
        {
            int penalty = Mathf.Min(Mathf.Abs(_currentGP), 100);
            _currentHP = INIT_HP - penalty;
            _currentGP = 50;
        }

        // GP 변경 동기화
        _photonView.RPC(nameof(RPC_ChangeGP), RpcTarget.All, _currentGP);
        OnGPChanged?.Invoke(_currentGP);
    }

    [PunRPC]
    private void RPC_ChangeHP(int hp, int life, int attacker, PhotonMessageInfo info)
    {
        DamageChecker.Instance.RPC_RequestDamage(hp, life, attacker, info.Sender.ActorNumber);
    }

    // 하위 호환성 (혹시 직접 호출되는 곳이 있을 수 있으므로)
    [PunRPC]
    private void RPC_ChangeGunpowder(int gunpowder, int life, int attacker, PhotonMessageInfo info)
    {
        RPC_ChangeHP(gunpowder, life, attacker, info);
    }

    [PunRPC]
    private void RPC_Dead(int killerForLog, bool isNormal, PhotonMessageInfo info)
    {
        DamageChecker.Instance.ActiveKillLog(killerForLog, isNormal, info.Sender.ActorNumber);
    }

    // 데미지 & 통계 관리 메서드
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

    [PunRPC]
    public void RPC_IncreaseTotalKillCount(int victimActorNumber, bool isLastKill, bool isNormalAttack)
    {
        if (_photonView.IsMine)
        {
            _totalKillCount++;

            PlayerEventManager.Instance.GetEvents(_photonView.OwnerActorNr).InvokeOnKillConfirmed(new KillContext
            {
                VictimActorNumber = victimActorNumber,
                IsLastKill = isLastKill,
                IsNormalAttack = isNormalAttack
            });
        }
    }

    public void ResetTotalKillCount()
    {
        _totalKillCount = 0;
    }

    private PhotonView FindAttackerPhotonView(int attackerActorNumber)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            PhotonView enemyPhotonView = enemy.GetComponent<PhotonView>();
            if (enemyPhotonView != null && enemyPhotonView.Owner != null &&
                enemyPhotonView.Owner.ActorNumber == attackerActorNumber)
            {
                return enemyPhotonView;
            }
        }

        return null;
    }

    public void RecordLastAttacker(int attackerActorNumber)
    {
        if (attackerActorNumber == _photonView.OwnerActorNr)
        {
            return;
        }

        _lastAttackerActorNumber = attackerActorNumber;
        _lastAttackTime = Time.time;
    }

    public int GetValidLastAttacker()
    {
        if (_lastAttackerActorNumber == -1)
        {
            return -1;
        }

        if (Time.time - _lastAttackTime > _attackTrackingDuration)
        {
            _lastAttackerActorNumber = -1;
            return -1;
        }

        return _lastAttackerActorNumber;
    }

    // ===== 궁극기 게이지 관리 메서드 =====

    public void IncreaseUltimateGauge(float amount)
    {
        if (amount <= 0f) return;
        if (_currentUltimateGauge >= _maxUltimateGauge) return;

        _currentUltimateGauge = Mathf.Min(_currentUltimateGauge + amount, _maxUltimateGauge);
        OnUltimateGaugeChanged?.Invoke(_currentUltimateGauge, _maxUltimateGauge);
    }

    public void ResetUltimateGauge()
    {
        _currentUltimateGauge = 0f;
        OnUltimateGaugeChanged?.Invoke(_currentUltimateGauge, _maxUltimateGauge);
    }

    public void SetMaxUltimateGauge(float max)
    {
        _maxUltimateGauge = Mathf.Max(1f, max);
        _currentUltimateGauge = Mathf.Min(_currentUltimateGauge, _maxUltimateGauge);
        OnUltimateGaugeChanged?.Invoke(_currentUltimateGauge, _maxUltimateGauge);
    }

    /// <summary>
    /// 데미지를 입혔을 때 공격자 궁극기 게이지 충전 (데미지의 30%)
    /// </summary>
    public void ChargeUltimateOnDamageDealt(int damage)
    {
        if (damage <= 0) return;
        IncreaseUltimateGauge(damage * ULTIMATE_GAUGE_RATE_ON_DAMAGE_DEALT);
    }

    /// <summary>
    /// 데미지를 입었을 때 피격자 궁극기 게이지 충전 (데미지의 50%)
    /// </summary>
    public void ChargeUltimateOnDamageTaken(int damage)
    {
        if (damage <= 0) return;
        IncreaseUltimateGauge(damage * ULTIMATE_GAUGE_RATE_ON_DAMAGE_TAKEN);
    }

    [PunRPC]
    public void RPC_ChargeUltimateOnDamageDealt(int damage)
    {
        if (!_photonView.IsMine) return;
        ChargeUltimateOnDamageDealt(damage);
    }

    // 플레이어 상태 관리 메서드
    public void ResurrectPlayerStat()
    {
        if (!_photonView.IsMine)
        {
            return;
        }

        // HP는 캐리오버에서 이미 설정됨 (DecreaseHP 내 ApplyResurrectCarryover)
        // 여기서는 상태만 초기화

        // 플레이어 상태 초기화
        _isRunning = false;
        _isJumping = false;
        _myMoveSpeed = _moveSpeed;
        _jumpCount = 0;
        _facingDirection = 1;
        _isFallingDead = false;

        // 최근 공격자 정보 초기화
        _lastAttackerActorNumber = -1;
        _lastAttackTime = 0f;

        // 네트워크 동기화
        _photonView.RPC(nameof(RPC_ChangeHP), RpcTarget.All, _currentHP, _currentPlayerLife, 0);
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
