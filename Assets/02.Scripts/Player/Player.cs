using UnityEngine;
using System.Collections.Generic;
using System;
using RaycastPro.RaySensors2D;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using System.Collections;



public class Player : MonoBehaviourPun, IDamagable
{
    [SerializeField]
    private List<Animator> _myAnimatorList;
    public List<Animator> MyAnimatorList => _myAnimatorList;

    [Header("DieParts")]
    [SerializeField]
    private List<GameObject> _myDiePartList;
    public List<GameObject> MyDiePartList => _myDiePartList;
    private List<GameObject> _headPartList;
    public List<GameObject> HeadPartList => _headPartList;
    private List<GameObject> _bodyPartList;
    public List<GameObject> BodyPartList => _bodyPartList;
    private List<GameObject> _leftArmPartList;
    public List<GameObject> LeftArmPartList => _leftArmPartList;
    private List<GameObject> _leftLegPartList;
    public List<GameObject> LeftLegPartList => _leftLegPartList;
    private List<GameObject> _rightArmPartList;
    public List<GameObject> RightArmPartList => _rightArmPartList;
    private List<GameObject> _rightLegPartList;
    public List<GameObject> RightLegPartList => _rightLegPartList;

    private Rigidbody2D _rigidbody2D;
    public Rigidbody2D Rigidbody2D => _rigidbody2D;

    private PlayerStat _playerStat;
    public PlayerStat PlayerStat => _playerStat;

    public PhotonView PhotonView;

    public Dictionary<EItemType, ItemDTO> EquipedItemDict;

    [Header("Bomb")]
    // 폭탄 스폰 위치 리스트
    // 0 45 90 135 180 225 270 315
    [SerializeField]
    private List<Transform> _bombSpawnPointList;
    [SerializeField]
    private List<Transform> _explosionSpawnPointList;

    private Bomb _normalBomb;
    public Bomb NormalBomb => _normalBomb;
    [SerializeField]
    private Bomb _dashBomb;
    public Bomb DashBomb => _dashBomb;
    private Bomb _headBomb;
    public Bomb HeadBomb => _headBomb;

    [Header("Timer")]
    [SerializeField]
    private float _attackTimer = 0f;
    public float AttackTimer => _attackTimer;
    [SerializeField]
    private float _gunPowderDecreaseTimer = 0f;
    public float GunPowderDecreaseTimer => _gunPowderDecreaseTimer;
    [SerializeField]
    private float _gunPowderDecreaseWithoutAttackTimer;
    public float GunPowderDecreaseWithoutAttackTimer => _gunPowderDecreaseWithoutAttackTimer;

    [Header("HitStop")]
    [SerializeField]
    private Vector2 _storedVelocity = Vector2.zero;
    public Vector2 StoredVelocity => _storedVelocity;
    [SerializeField]
    private bool _hasStoredVelocity = false;
    public bool HasStoredVelocity => _hasStoredVelocity;

    [Header("GunPowder")]
    [SerializeField]
    private float _gunPowderSpreadAngle = 90f;
    private float _gunPowderSpreadDistance = 1.0f;

    public event Action OnAttack;
    public event Action OnHit;

    [SerializeField]
    private BoxRay2D _groundRay2D;
    public BoxRay2D GroundRay2D => _groundRay2D;

    public GameObject HeadBombPrefab;
    public GameObject GunPowderPrefab;
    public GameObject DieExplosionPrefab;
    public GameObject HitEffectPrefab;
    public GameObject UltimateEffectPrefab;


    private const int RANDOM_SEED = 123456;
    private const string BASIC_BOMB_ID =  "BO0001";
    public BombStat BasicBombStat;
    public BombStat SpecialBombStat;

    // 쿨타임 체크용 변수
    private float _lastNormalBombTime = 0f;
    private float _lastSpecialBombTime = 0f;

    public float LastNormalBombTime => _lastNormalBombTime;
    public float LastSpecialBombTime => _lastSpecialBombTime;

    private Ultimate _ultimate;
    public Ultimate Ultimate => _ultimate;
    private bool _ultimateEffectOn = false;

    private PlayerMaterial _playerMaterial;


    
    [SerializeField] private float _ultimateChanceTimer = 0f; // 내부 타이머(갱신/소모 로직은 별도 구현 예정)
    public float UltimateChanceTimer { get => _ultimateChanceTimer; set => _ultimateChanceTimer = value; }

    private void Awake()
    {
        _playerStat = GetComponent<PlayerStat>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        PhotonView = GetComponent<PhotonView>();
        _playerMaterial = GetComponent<PlayerMaterial>();

        EquipedItemDict = new Dictionary<EItemType, ItemDTO>();
        LoadItems();

        // 기본 폭탄 정보 가져오기
        GameObject basicBomb = ItemDatabase.Instance.GetItem(BASIC_BOMB_ID).Prefab;
        _normalBomb = basicBomb.GetComponent<Bomb>();
        BasicBombStat = ItemDatabase.Instance.GetStat<BombStat>(BASIC_BOMB_ID);
        
        UI_PingBase.Instance.SetPing(this.transform);

        UnityEngine.Random.InitState(RANDOM_SEED);

        EventManager.Instance.OnPlayerItemChanged += LoadItems;

        InitializeBodyParts();
    }

    private void InitializeBodyParts()
    {
        _headPartList = new List<GameObject>();
        _bodyPartList = new List<GameObject>();
        _leftArmPartList = new List<GameObject>();
        _leftLegPartList = new List<GameObject>();
        _rightArmPartList = new List<GameObject>();
        _rightLegPartList = new List<GameObject>();

        _headPartList.Add(_myDiePartList[0]);
        _headPartList.Add(_myDiePartList[2]);
        _headPartList.Add(_myDiePartList[3]);

        _bodyPartList.Add(_myDiePartList[1]);

        _leftArmPartList.Add(_myDiePartList[4]);
        _leftLegPartList.Add(_myDiePartList[5]);

        _rightArmPartList.Add(_myDiePartList[6]);
        _rightLegPartList.Add(_myDiePartList[7]);
    }

    private void LoadItems()
    {
        PhotonPlayer photonPlayer = PhotonView.Owner;
        for (int i = 0; i < (int)EItemType.None; i++)
        {
            EItemType itemType = (EItemType)i;
            if (photonPlayer.CustomProperties.TryGetValue(itemType.ToString(), out object itemID))
            {
                if (EquipedItemDict.ContainsKey((EItemType)i))
                {
                    EquipedItemDict[(EItemType)i] = ItemDatabase.Instance.GetItem((string)itemID);
                }
                else
                {
                    EquipedItemDict.Add((EItemType)i, ItemDatabase.Instance.GetItem((string)itemID));
                }
            }
        }
        
        // 특수폭탄 정보 받아오기
        SpecialBombStat = ItemDatabase.Instance.GetStat<BombStat>(EquipedItemDict[EItemType.Bomb].ID);

        if(UltimateManager.Instance != null)
        {
            _ultimate = UltimateManager.Instance.GetUltimate(EquipedItemDict[EItemType.Bomb].ID, this);
        }
    }

    private void Start()
    {
        // 1. 이벤트 핸들러 등록
        _playerStat.OnGunPowderEmpty += HandleGunPowderEmpty;

        // 2. Rigidbody2D 최적화된 초기화
        if (photonView.IsMine)
        {
            //_rigidbody2D.gravityScale = 4; // 기본 중력값
            _rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate; // 보간 활성화
        }
        else
        {
            //_rigidbody2D.gravityScale = 0;
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody2D.interpolation = RigidbodyInterpolation2D.None; // 보간 비활성화
        }
        
        _rigidbody2D.linearVelocity = Vector2.zero;
        _rigidbody2D.angularVelocity = 0f;

        // 3. PlayerStat 초기화
        _playerStat.InitializeStats(); // 체력, 건파우더 등 기본값 세팅

        // 4. 타이머 초기화
        _attackTimer = 0f;
        _gunPowderDecreaseTimer = 0f;
        _gunPowderDecreaseWithoutAttackTimer = 0f;
        _ultimateChanceTimer = 0f;

        if(PhotonView.IsMine)
        {
            gameObject.tag = "Player";
            //gameObject.layer = LayerMask.NameToLayer("Player");
            
        }
        else
        {   
            gameObject.tag = "Enemy";   
            //gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
    }

    private void OnDisable()
    {
        // 이벤트 핸들러 해제 (중복 방지)
        _playerStat.OnGunPowderEmpty -= HandleGunPowderEmpty;
    }

    public void ResurrectPlayer()
    {
        // 각종 타이머들 초기화
        _attackTimer = 0f;
        _gunPowderDecreaseTimer = 0f;
        _gunPowderDecreaseWithoutAttackTimer = 0f;
        _lastNormalBombTime = 0f;
        _lastSpecialBombTime = 0f;
        _ultimateChanceTimer = 0f;

        // 저장된 속도 상태 초기화
        ClearStoredVelocity();

        // 플레이어 스탯 초기화 (건파우더 초기화)
        _playerStat.ResurrectPlayerStat();
    }

    private void HandleGunPowderEmpty()
    {
        // PlayerFSM을 통해 SyncStateChange 호출
        PlayerFSM playerFSM = GetComponent<PlayerFSM>();
        if (playerFSM != null)
        {
            // 네트워크 동기화된 상태 변경
            playerFSM.SyncStateChange<PlayerDieState>();
        }
        else
        {
            // PlayerFSM이 없는 경우 직접 변경
            GetComponent<PlayerFSM>().ChangeState<PlayerDieState>();
        }
    }

    private void Update()
    {
        if (!PhotonView.IsMine)
        {
            return;
        }
        _attackTimer += Time.deltaTime;

        _gunPowderDecreaseTimer += Time.deltaTime;

        DecreaseGunPowderPeriodically();

        _gunPowderDecreaseWithoutAttackTimer += Time.deltaTime;
        DecreaseGunPowderWithoutAttack();

        UltimateChanceTimerUpdate();
    }

    private void UltimateChanceTimerUpdate()
    {
        // 궁극기 사용가능 상태
        if (_playerStat.HasUltimateChance)
        {
            if (!_ultimateEffectOn)
            {
                if (UltimateEffectPrefab != null && !UltimateEffectPrefab.activeSelf)
                {
                    RPC_UltimateEffect(true);
                    RPC_SetMaterial((byte)EPlayerMaterial.Ultimate);
                }
                _ultimateEffectOn = true;
            }

            _ultimateChanceTimer += Time.deltaTime;
            if (_ultimateChanceTimer >= _playerStat.UltimateChanceDuration)
            {
                _playerStat.HasUltimateChance = false;
                _playerStat.HasUsedUltimateThisLife = true;
                _ultimateChanceTimer = 0f;
                RPC_UltimateEffect(false);
                RPC_SetMaterial((byte)EPlayerMaterial.Default);
                _ultimateEffectOn = false;
            }
        }
    }
    
    public void RPC_UltimateEffect(bool isOn)
    {
        if(!PhotonView.IsMine)
        {
            return;
        }
        PhotonView.RPC(nameof(UltimateEffect), RpcTarget.All, isOn);
    }

    [PunRPC]
    public void UltimateEffect(bool isOn)
    {
        UltimateEffectPrefab.SetActive(isOn);
    }

    [PunRPC]
    public void SetMaterial(byte id)
    {
        if (_playerMaterial == null)
        {
            _playerMaterial = GetComponent<PlayerMaterial>();
        }
        if (_playerMaterial == null)
        {
            return;
        }
        _playerMaterial.ApplyMaterialById(id, _playerStat.MySpriteREndererList);
    }

    public void RPC_SetMaterial(byte id)
    {
        if(!PhotonView.IsMine)
        {
            return;
        }
        PhotonView.RPC(nameof(SetMaterial), RpcTarget.All, id);
    }

    public void ExecuteUltimate()
    {
        if(_playerStat.HasUltimateChance && !_playerStat.HasUsedUltimateThisLife)
        {
            if(_ultimate == null)
            {
                Debug.LogError("궁극기 스크립트가 없습니다.");
                return;
            }
            _ultimate.ExcuteUltimate();
            _playerStat.HasUsedUltimateThisLife = true;
            _playerStat.HasUltimateChance = false;
            _ultimateChanceTimer = 0f;
        }
    }

    /// <summary>
    /// 주기적으로 건파우더 감소
    /// </summary>
    private void DecreaseGunPowderPeriodically()
    {
        if (_gunPowderDecreaseTimer >= PlayerStat.GunPowderDecreaseTime)
        {
            _gunPowderDecreaseTimer = 0f;
            //PhotonView.RPC(nameof(DecreaseGunPowder), RpcTarget.All, 1);
            _playerStat.DecreaseGunPowderCount(1, photonView.OwnerActorNr);

        }
    }

    /// <summary>
    /// 공격을 일정시간 하지 않으면 건파우더 감소
    /// </summary>
    private void DecreaseGunPowderWithoutAttack()
    {
        if (_gunPowderDecreaseWithoutAttackTimer >= PlayerStat.AttackPenaltyTime)
        {
            _gunPowderDecreaseWithoutAttackTimer = 0f;
            //PhotonView.RPC(nameof(DecreaseGunPowder), RpcTarget.All, PlayerStat.AttackPenaltyAmount);
            _playerStat.DecreaseGunPowderCount(PlayerStat.AttackPenaltyAmount, photonView.OwnerActorNr);
        }
    }

    /// <summary>
    /// 공격을 하면 타이머 초기화
    /// </summary>
    public void ResetGunPowderDecreaseWithoutAttackTimer()
    {
        _gunPowderDecreaseWithoutAttackTimer = 0f;
    }

    public void TakeDamage(int damage, Vector3 attackerBomb, int attackerViewId, int attackerActorNumber, bool isFallingOut)
    {
        // 피격 VFX 재생
        if (tag == "Player")
        {
            VFXPool.Instance.RandomPlay("Damaged", transform.position, 1, 3);
        }
        else
        {
            VFXPool.Instance.RandomPlay("Hit", transform.position, 1, 6);
        }
        SoundManager.Instance.PlayLocalRandomSound("PlayerDamage", transform, 1, 7, 0f, false, SoundType.SFX, true, 1f, 50f);
        SoundManager.Instance.PlayLocalRandomSound("PlayerDamageVoice", transform, 1, 4, 0f, false, SoundType.SFX, true, 1f, 50f);

        if (!PhotonView.IsMine)
        {
            return;
        }
        PhotonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damage, attackerBomb, attackerViewId, attackerActorNumber, isFallingOut);
    }

    [PunRPC]
    public void RPC_TakeDamage(int damage, Vector3 attackerBomb, int attackerViewId, int attackerActorNumber, bool isFallingOut,PhotonMessageInfo info)
    {
        if(_playerStat.IsImmune)
        {
            return;
        }

        // 체력 감소
        bool isDead = _playerStat.DecreaseGunPowderCount(damage, attackerActorNumber);

        // 날 때린 사람 딜량 증가
        PhotonView attackerView = PhotonView.Find(attackerViewId);
        if(attackerView != null)
        {
            attackerView.GetComponent<PlayerStat>().IncreaseTotalDamage(damage);
        }
        if(isDead)
        {
            attackerView.GetComponent<PlayerStat>().IncreaseTotalKillCount();
        }
        
        // Gunpowder 낙출
        ReleaseGunPowder(attackerBomb, attackerViewId, damage, _gunPowderSpreadAngle, _gunPowderSpreadDistance, isFallingOut);

        // 피격 횟수 증가
        _playerStat.IncreseDamagedCount();

        // 피격 이벤트 발생
        OnHit?.Invoke();
    }

    /// <summary>
    /// 피격시 건파우더 흩뿌리기
    /// </summary>
    // [PunRPC]
    public void ReleaseGunPowder(Vector3 explosionOrigin, int attackerViewId, int count = 3, float spreadAngle = 30f,
     float distance = 1.0f, bool isFallingOut = true)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // attackerViewId로 Transform 찾기
        Transform attacker = null;
        PhotonView attackerView = PhotonView.Find(attackerViewId);
        if (attackerView != null)
            attacker = attackerView.transform;

        Vector3 baseDir = (transform.position - explosionOrigin).normalized;

        for (int i = 0; i < count; i++)
        {
            float angle = (i - (count - 1) / 2f) * spreadAngle;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 dir = rot * baseDir;
            Vector3 spawnPos = transform.position + dir * distance;
            spawnPos.z = 0f;

            // 랜덤 시드 추가 (시간 + 인덱슬 고유값 생성)
            int randomSeed =UnityEngine.Random.Range(0, 9999);


            object[] instData = new object[] { attackerViewId, isFallingOut, randomSeed, PhotonView.ViewID };
            PhotonNetwork.Instantiate(GunPowderPrefab.name, spawnPos, Quaternion.identity, 0, instData);
        }
    }

    public void RPC_ReleaseGunPowder(Vector3 explosionOrigin, int attackerViewId, int count = 3, float spreadAngle = 30f,
     float distance = 1.0f, bool isFallingOut = true)
    {
        if(!PhotonView.IsMine)
        {
            return;
        }

        PhotonView.RPC(nameof(ReleaseGunPowder), RpcTarget.All, explosionOrigin, attackerViewId, count, spreadAngle, distance, isFallingOut);
    }

    /// <summary>
    /// 키보드 입력에 따라 폭탄 스폰 위치 반환
    /// 8방향으로 나누어져 있다.
    /// </summary>
    /// <returns></returns>
    public Transform GetBombSpawnPoint()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        switch ((h, v))
        {
            case (1, 0):
                return _bombSpawnPointList[(int)EBombSpawnPoint.Right];
            case (1, 1):
                return _bombSpawnPointList[(int)EBombSpawnPoint.RightUp];
            case (0, 1):
                return _bombSpawnPointList[(int)EBombSpawnPoint.Up];
            case (-1, 1):
                return _bombSpawnPointList[(int)EBombSpawnPoint.LeftUp];
            case (-1, 0):
                return _bombSpawnPointList[(int)EBombSpawnPoint.Left];
            case (-1, -1):
                return _bombSpawnPointList[(int)EBombSpawnPoint.LeftDown];
            case (0, -1):
                return _bombSpawnPointList[(int)EBombSpawnPoint.Down];
            case (1, -1):
                return _bombSpawnPointList[(int)EBombSpawnPoint.RightDown];
            default:
                return _playerStat.FacingDirection == 1 ? _bombSpawnPointList[(int)EBombSpawnPoint.Right] : _bombSpawnPointList[(int)EBombSpawnPoint.Left];
        }
    }

    public Transform GetExplosionSpawnPoint()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        switch ((h, v))
        {
            case (-1, 0):
                return _explosionSpawnPointList[(int)EBombSpawnPoint.Right];
            case (-1, -1):
                return _explosionSpawnPointList[(int)EBombSpawnPoint.RightUp];
            case (0, -1):
                return _explosionSpawnPointList[(int)EBombSpawnPoint.Up];
            case (1, -1):
                return _explosionSpawnPointList[(int)EBombSpawnPoint.LeftUp];
            case (1, 0):
                return _explosionSpawnPointList[(int)EBombSpawnPoint.Left];
            case (1, 1):
                return _explosionSpawnPointList[(int)EBombSpawnPoint.LeftDown];
            case (0, 1):
                return _explosionSpawnPointList[(int)EBombSpawnPoint.Down];
            case (-1, 1):
                return _explosionSpawnPointList[(int)EBombSpawnPoint.RightDown];
            default:
                return _explosionSpawnPointList[(int)EBombSpawnPoint.Down];
        }
    }

    public Transform GetBombSpawnPoint(EBombSpawnPoint spawnPoint)
    {
        return _bombSpawnPointList[(int)spawnPoint];
    }

    [PunRPC]
    public void SetAnimatorTrigger(string triggerName)
    {
        foreach (Animator animator in _myAnimatorList)
        {
            animator.SetTrigger(triggerName);
        }
    }

    [PunRPC]
    public void SetAnimatorBool(string boolName, bool value)
    {
        foreach (Animator animator in _myAnimatorList)
        {
            animator.SetBool(boolName, value);
        }
    }

    [PunRPC]
    public void RPC_SetAnimatorTrigger(string triggerName)
    {
        if(!PhotonView.IsMine)
        {
            return;
        }

        PhotonView.RPC(nameof(SetAnimatorTrigger), RpcTarget.All, triggerName);
    }

    [PunRPC]
    public void RPC_SetAnimatorBool(string boolName, bool value)
    {
        if(!PhotonView.IsMine)
        {
            return;
        }

        PhotonView.RPC(nameof(SetAnimatorBool), RpcTarget.All, boolName, value);
    }

    [PunRPC]
    public void ResetAnimatorTrigger(string triggerName)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        foreach (Animator animator in _myAnimatorList)
        {
            animator.ResetTrigger(triggerName);
        }
    }

    [PunRPC]
    public void RPC_ResetAnimatorTrigger(string triggerName)
    {
        if(!PhotonView.IsMine)
        {
            return;
        }

        PhotonView.RPC(nameof(ResetAnimatorTrigger), RpcTarget.All, triggerName);
    }

    public void RPC_SetFacingDirection(int direction)
    {
        PhotonView.RPC(nameof(SetFacingDirection), RpcTarget.All, direction);
    }

    [PunRPC]
    public void SetFacingDirection(int direction)
    {
        _playerStat.FacingDirection = direction;
        foreach (SpriteRenderer spriteRenderer in _playerStat.MySpriteREndererList)
        {
            if (_playerStat.FacingDirection == 1)
            {
                spriteRenderer.flipX = false;
            }
            else
            {
                spriteRenderer.flipX = true;
            }
        }
    }

    [PunRPC]
    public void RPC_ChangeState(string stateName)
    {
        // 현재 상태가 사망 상태인 경우, 부활 전까지 다른 상태로 전환을 막는다
        PlayerFSM fsmForGuard = GetComponent<PlayerFSM>();
        if (fsmForGuard != null)
        {
            if (fsmForGuard.IsCurrentState<PlayerDieState>() && stateName != nameof(PlayerIdleState))
            {
                return;
            }
        }
        
        // PlayerFSM 컴포넌트를 찾아서 상태 변경
        PlayerFSM playerFSM = GetComponent<PlayerFSM>();
        if (playerFSM != null)
        {
            // 다른 클라이언트에서 상태 변경
            switch (stateName)
            {
                case "PlayerIdleState":
                    playerFSM.ChangeState<PlayerIdleState>();
                    break;
                case "PlayerWalkState":
                    playerFSM.ChangeState<PlayerWalkState>();
                    break;
                case "PlayerJumpState":
                    playerFSM.ChangeState<PlayerJumpState>();
                    break;
                case "PlayerDashState":
                    playerFSM.ChangeState<PlayerDashState>();
                    break;
                case "PlayerRunState":
                    playerFSM.ChangeState<PlayerRunState>();
                    break;
                case "PlayerBreakState":
                    playerFSM.ChangeState<PlayerBreakState>();
                    break;
                case "PlayerJumpDashState":
                    playerFSM.ChangeState<PlayerJumpDashState>();
                    break;
                case "PlayerRecoilState":
                    playerFSM.ChangeState<PlayerRecoilState>();
                    break;
                case "PlayerDamagedState":
                    playerFSM.ChangeState<PlayerDamagedState>();
                    break;
                case "PlayerNormalRecoilState":
                    playerFSM.ChangeState<PlayerNormalRecoilState>();
                    break;
                case "PlayerDieState":
                    playerFSM.ChangeState<PlayerDieState>();
                    break;
                case "PlayerFallDeadState":
                    playerFSM.ChangeState<PlayerFallDeadState>();
                    break;
                case "PlayerHitStopState":
                    playerFSM.ChangeState<PlayerHitStopState>();
                    break;
                case "PlayerFallState":
                    playerFSM.ChangeState<PlayerFallState>();
                    break;
                default:
                    break;
            }
        }
        else
        {
            Debug.LogError("PlayerFSM component not found!");
        }
    }

    public void InvokeAttack()
    {
        OnAttack?.Invoke();
    }

    
    public bool CanNormalBomb()
    {
        return AttackTimer - _lastNormalBombTime >= BasicBombStat.CoolTime;
    }

    public bool CanSpecialBomb()
    {
        return AttackTimer - _lastSpecialBombTime >= SpecialBombStat.CoolTime;
    }

    public void SetLastNormalBombTime()
    {
        _lastNormalBombTime = AttackTimer;
    }

    public void SetLastSpecialBombTime()
    {
        _lastSpecialBombTime = AttackTimer;
    }

    /// <summary>
    /// 히트스탑 중에 받은 속도를 저장
    /// </summary>
    public void StoreVelocity()
    {
        if (_rigidbody2D != null)
        {
            _storedVelocity = _rigidbody2D.linearVelocity;
            _hasStoredVelocity = true;
        }
    }

    /// <summary>
    /// 저장된 속도를 복원하고 저장 상태 초기화
    /// </summary>
    public void RestoreVelocity()
    {
        if (_hasStoredVelocity && _rigidbody2D != null)
        {
            _rigidbody2D.linearVelocity = _storedVelocity;

            _hasStoredVelocity = false;
            _storedVelocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 저장된 속도 상태 초기화
    /// </summary>
    public void ClearStoredVelocity()
    {
        _hasStoredVelocity = false;
        _storedVelocity = Vector2.zero;
    }

    [PunRPC]
    public void RPC_SetIsImmune(bool isImmune)
    {
        _playerStat.IsImmune = isImmune;
        Debug.Log($"[RPC_SetIsImmune] Player {PhotonView.Owner.ActorNumber} - IsImmune set to: {isImmune}");
    }

    public void SetDownJump()
    {
        gameObject.layer = LayerMask.NameToLayer("DownJump");
        // 하위 오브젝트 들도 모드 변경
        foreach(Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("DownJump");
        }
        StartCoroutine(ResetDownJump());
    }

    public IEnumerator ResetDownJump()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.layer = LayerMask.NameToLayer("Player");
        _playerStat.IsDownJump = false;
        foreach(Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }
    
    public void Observe()
    {
        // 관전 상태가 되서 상호작용도 안하고 모습도 안보이게 해야함

        /*
        foreach(SpriteRenderer spriteRenderer in _playerStat.MySpriteREndererList)
        {
            spriteRenderer.enabled = false;
        }*/

        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
    }
}
