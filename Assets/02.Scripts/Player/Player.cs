using UnityEngine;
using System.Collections.Generic;
using System;
using RaycastPro.RaySensors2D;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using System.Collections;
using DG.Tweening;



public class Player : MonoBehaviourPun, IDamagable
{
    [SerializeField]
    private List<Animator> _myAnimatorList;
    public List<Animator> MyAnimatorList => _myAnimatorList;

    [Header("죽음 파츠")]
    [SerializeField]
    private List<GameObject> _diePartList;
    public List<GameObject> DiePartList => _diePartList;
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

    [Header("폭탄 설정")]
    // 폭탄 스폰 위치 리스트 (각도: 0,45,90,135,180,225,270,315)
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

    [Header("타이머")]
    [SerializeField]
    private float _attackTimer = 0f;
    public float AttackTimer => _attackTimer;
    [SerializeField]
    private float _gunPowderDecreaseTimer = 0f;
    public float GunPowderDecreaseTimer => _gunPowderDecreaseTimer;
    [SerializeField]
    private float _gunPowderDecreaseWithoutAttackTimer;
    public float GunPowderDecreaseWithoutAttackTimer => _gunPowderDecreaseWithoutAttackTimer;
    [SerializeField]
    private float _colorUpdateWithoutAttackTimer = 0f;
    // legacy: moved to PlayerSFXAnimationEvent
    private float _warningSfxTimer = 0f;

    private Tween _preExplosionPulseTween;
    private Vector3 _defaultLocalScale;
    private Dictionary<SpriteRenderer, Color> _originalColorMap; // 게임 시작 시 저장되는 진짜 원본 색상

    [Header("히트스탑")]
    [SerializeField]
    private Vector2 _storedVelocity = Vector2.zero;
    public Vector2 StoredVelocity => _storedVelocity;
    [SerializeField]
    private bool _hasStoredVelocity = false;
    public bool HasStoredVelocity => _hasStoredVelocity;

    [Header("건파우더 설정")]
    [SerializeField]
    private float _gunPowderSpreadAngle = 90f;
    private float _gunPowderSpreadDistance = 1.0f;

    public event Action OnAttack;
    public event Action OnHit;

    [SerializeField]
    private BoxRay2D _groundRay2D;
    public BoxRay2D GroundRay2D => _groundRay2D;

    [Header("프리팹 참조")]
    public GameObject HeadBombPrefab;
    public GameObject GunPowderPrefab;
    public GameObject DieExplosionPrefab;
    public GameObject HitEffectPrefab;
    public GameObject UltimateEffectPrefab;
    public GameObject ExplosionEffectPrefab;
    public GameObject FallDeadVFXPrefab;


    private const int RANDOM_SEED = 123456;
    private const string BASIC_BOMB_ID = "BO0001";
    
    // 공격 없을 때 관련 상수
    private const float COLOR_UPDATE_TICK_SECONDS = 0.5f;
    private const float REDNESS_START_RATIO = 0.4f;
    private const float WARNING_RATIO_THRESHOLD = 0.7f;
    private const float MAX_RED_SATURATION = 0.6f;
    private const float PULSE_SCALE_MULTIPLIER = 1.2f;
    private const float PULSE_HALF_DURATION = 0.2f;
    private const float WARNING_INTERVAL_MAX = 0.7f;
    private const float WARNING_INTERVAL_MIN = 0.1f;
    private const float WARNING_PITCH_MIN = 1.0f;
    private const float WARNING_PITCH_MAX = 1.9f;
    private const int NO_ATTACK_RELEASE_COUNT = 10;
    private const float NO_ATTACK_RELEASE_SPREAD_ANGLE = 30f;
    private const float NO_ATTACK_RELEASE_DISTANCE = 1.0f;
    public BombStat BasicBombStat;
    public BombStat SpecialBombStat;

    // 쿨타임 체크용 변수
    private float _lastNormalBombTime = -999f;
    private float _lastSpecialBombTime = -999f;

    public float LastNormalBombTime => _lastNormalBombTime;
    public float LastSpecialBombTime => _lastSpecialBombTime;

    private Ultimate _ultimate;
    public Ultimate Ultimate => _ultimate;
    private bool _ultimateEffectOn = false;
    private Coroutine _ultimateEffectOffRoutine;

    private PlayerMaterial _playerMaterial;
    private PlayerFSM _playerFSM;
    public PlayerFSM PlayerFSM => _playerFSM;
    private DamagePopup _damagePopup;
    public DamagePopup DamagePopup => _damagePopup;

    public AirDropItemLootVFX AirDropItemLootVFX;
    private AirDropItemBase _airDropItem;
    public AirDropItemBase AirDropItem => _airDropItem;


    [SerializeField]
    private PlayerSFXAnimationEvent _playerSFXAnimationEvent;
    public PlayerSFXAnimationEvent PlayerSFXAnimationEvent => _playerSFXAnimationEvent;




    // 대시 탭 타임
    public float LastDashTapTimeLeft = -999f;
    public float LastDashTapTimeRight = -999f;

    [SerializeField] private float _ultimateChanceTimer = 0f; // 내부 타이머(갱신/소모 로직은 별도 구현 예정)
    public float UltimateChanceTimer { get => _ultimateChanceTimer; set => _ultimateChanceTimer = value; }

    private void Awake()
    {
        _playerStat = GetComponent<PlayerStat>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        PhotonView = GetComponent<PhotonView>();
        _playerMaterial = GetComponent<PlayerMaterial>();
        _playerFSM = GetComponent<PlayerFSM>();
        _damagePopup = GetComponent<DamagePopup>();

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

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnPlayerItemChanged -= LoadItems;
        }
        if (_playerStat != null)
        {
            _playerStat.OnGunPowderEmpty -= HandleGunPowderEmpty;
            _playerStat.OnGunpowderIncreased -= HandleGunpowderIncreased;
        }
    }

    /// <summary>
    /// 플레이어의 신체 부위별 GameObject를 초기화하고 분류하는 메서드
    /// PlayerStat의 SpriteRenderer 리스트를 순회하며 BodyPartMarker 컴포넌트를 기반으로
    /// 나중에 추가될 부분도 BodyPartMarker 컴포넌트를 추가해줘야 함
    /// </summary>
    private void InitializeBodyParts()
    {
        // 각 신체 부위별 GameObject 리스트를 초기화
        _headPartList = new List<GameObject>();        // 머리 부위 리스트
        _bodyPartList = new List<GameObject>();        // 몸통 부위 리스트
        _leftArmPartList = new List<GameObject>();     // 왼팔 부위 리스트
        _leftLegPartList = new List<GameObject>();     // 왼다리 부위 리스트
        _rightArmPartList = new List<GameObject>();    // 오른팔 부위 리스트
        _rightLegPartList = new List<GameObject>();    // 오른다리 부위 리스트

        foreach (var part in _diePartList)
        {
            if (part == null) { continue; }  // null 체크

            // 해당 SpriteRenderer가 속한 GameObject에서 BodyPartMarker 컴포넌트 검색
            BodyPartMarker markerComp = part.GetComponent<BodyPartMarker>();
            if (markerComp == null) { continue; }  // BodyPartMarker가 없으면 스킵

            // 마커에서 정의된 신체 부위 타입 가져오기
            BodyPartType partType = markerComp.PartType;

            // 신체 부위 타입에 따라 해당하는 리스트에 GameObject 추가
            switch (partType)
            {
                case BodyPartType.Head:     // 머리 부위
                    _headPartList.Add(part);
                    break;
                case BodyPartType.Body:     // 몸통 부위
                    _bodyPartList.Add(part);
                    break;
                case BodyPartType.LeftArm:  // 왼팔 부위
                    _leftArmPartList.Add(part);
                    break;
                case BodyPartType.LeftLeg:  // 왼다리 부위
                    _leftLegPartList.Add(part);
                    break;
                case BodyPartType.RightArm: // 오른팔 부위
                    _rightArmPartList.Add(part);
                    break;
                case BodyPartType.RightLeg: // 오른다리 부위
                    _rightLegPartList.Add(part);
                    break;
            }
        }

        // 모든 신체 부위 리스트가 비어있는 경우 경고 메시지 출력
        if ((_headPartList.Count + _bodyPartList.Count + _leftArmPartList.Count + _leftLegPartList.Count + _rightArmPartList.Count + _rightLegPartList.Count) == 0)
        {
            Debug.LogWarning("BodyPartMarker를 찾지 못했습니다. PlayerSprites 루트에 마커를 추가해주세요.");
        }
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

        if (UltimateManager.Instance != null)
        {
            _ultimate = UltimateManager.Instance.GetUltimate(EquipedItemDict[EItemType.Bomb].ID, this);
        }
    }

    private void Start()
    {
        // 1. 이벤트 핸들러 등록
        _playerStat.OnGunPowderEmpty += HandleGunPowderEmpty;
        _playerStat.OnGunpowderIncreased += HandleGunpowderIncreased;

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
        _colorUpdateWithoutAttackTimer = 0f;
        _ultimateChanceTimer = 0f;

        if (PhotonView.IsMine)
        {
            gameObject.tag = "Player";
            //gameObject.layer = LayerMask.NameToLayer("Player");

        }
        else
        {
            gameObject.tag = "Enemy";
            //gameObject.layer = LayerMask.NameToLayer("Enemy");
        }

        _defaultLocalScale = transform.localScale;

        // 원본 색상 저장 (게임 시작 시 한 번만)
        InitializeOriginalColors();

        // 로컬 필드 팀을 항상 네트워크 프로퍼티와 동기화
        if (PhotonView.Owner != null && PhotonView.Owner.CustomProperties.ContainsKey(EProperties.Team.ToString()))
        {
            _playerStat.Team = (EInGameTeam)PhotonView.Owner.CustomProperties[EProperties.Team.ToString()];
        }
    }

    void OnEnable()
    {
        if (_playerFSM != null)
        {
            _playerFSM.SyncStateChange<PlayerIdleState>();
        }
    }

    /// <summary>
    /// 게임 시작 시 원본 색상을 저장합니다. (한 번만 실행)
    /// </summary>
    private void InitializeOriginalColors()
    {
        _originalColorMap = new Dictionary<SpriteRenderer, Color>();
        
        foreach (var renderer in _playerStat.MySpriteREndererList)
        {
            if (renderer != null)
            {
                _originalColorMap[renderer] = renderer.color;
            }
        }
    }

    public void ResurrectPlayer()
    {
        // 각종 타이머들 초기화
        _attackTimer = 0f;
        _gunPowderDecreaseTimer = 0f;
        _gunPowderDecreaseWithoutAttackTimer = 0f;
        _colorUpdateWithoutAttackTimer = 0f;
        _lastNormalBombTime = 0f;
        _lastSpecialBombTime = 0f;
        _ultimateChanceTimer = 0f;
        _warningSfxTimer = 0f;
        // legacy SFX state removed (moved to PlayerSFXAnimationEvent)

        // 저장된 속도 상태 초기화
        ClearStoredVelocity();

        // 색상 및 펄스 효과 초기화
        ResetColorAndEffects();

        // 플레이어 스탯 초기화 (건파우더 초기화)
        _playerStat.ResurrectPlayerStat();
    }

    private void HandleGunpowderIncreased(int amount)
    {
        if (_playerSFXAnimationEvent != null)
        {
            _playerSFXAnimationEvent.OnGunpowderAbsorbed();
        }
    }

    private void HandleGunPowderEmpty()
    {
        // 죽을 때 색상 및 효과 초기화
        ResetColorAndEffects();
        
        // 궁극기 효과 초기화
        RPC_UltimateEffect(false);
        RPC_SetMaterial((byte)EPlayerMaterial.Default);
        _ultimateEffectOn = false;
        
        // 궁극기 관련 타이머 초기화
        _ultimateChanceTimer = 0f;
        
        // PlayerFSM을 통해 SyncStateChange 호출
        if (_playerFSM != null)
        {
            // 네트워크 동기화된 상태 변경
            _playerFSM.SyncStateChange<PlayerDieState>();
            return;
        }

        // PlayerFSM이 없는 경우 방어적으로 컴포넌트 조회 후 변경
        var fsm = GetComponent<PlayerFSM>();
        if (fsm != null)
        {
            fsm.ChangeState<PlayerDieState>();
        }
    }

    private void Update()
    {
        // 테스트
        // ------------------------------------------------------------
        if (!PhotonView.IsMine)
        {
            return;
        }
        
        _attackTimer += Time.deltaTime;


        // 대기방에서 작동 안하게 하기 위해 추가
        if (GameManager.Instance.CurrentGameState == EGameState.Waiting || GameManager.Instance.CurrentGameState == EGameState.GameOver)
        {
            return;
        }

        // 주기적으로 건파우더 감소
        /*
        _gunPowderDecreaseTimer += Time.deltaTime;

        DecreaseGunPowderPeriodically();*/

        // 공격 없을 때 건파우더 감소
        _gunPowderDecreaseWithoutAttackTimer += Time.deltaTime;
         DecreaseGunPowderWithoutAttack();

        // Gunpowder heal SFX window is managed in PlayerSFXAnimationEvent
        UpdateWarningSfx();

        UltimateChanceTimerUpdate();
    }

    /// <summary>
    /// 궁극기 사용 가능 상태 타이머 업데이트
    /// </summary>
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
        if (!PhotonView.IsMine)
        {
            return;
        }
        PhotonView.RPC(nameof(UltimateEffect), RpcTarget.All, isOn);
    }

    [PunRPC]
    public void UltimateEffect(bool isOn)
    {
        if (UltimateEffectPrefab == null)
        {
            return;
        }

        if (isOn)
        {
            if (_ultimateEffectOffRoutine != null)
            {
                StopCoroutine(_ultimateEffectOffRoutine);
                _ultimateEffectOffRoutine = null;
            }
            UltimateEffectPrefab.SetActive(true);
            PlayParticleGroup(UltimateEffectPrefab);
        }
        else
        {
            if (_ultimateEffectOffRoutine != null)
            {
                StopCoroutine(_ultimateEffectOffRoutine);
            }
            _ultimateEffectOffRoutine = StartCoroutine(StopParticleGroupThenDisable(UltimateEffectPrefab));
        }
    }

    private void PlayParticleGroup(GameObject root)
    {
        var particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem ps = particleSystems[i];
            if (ps == null) { continue; }
            var emission = ps.emission;
            emission.enabled = true;
            ps.Play(true);
        }
    }

    private IEnumerator StopParticleGroupThenDisable(GameObject root)
    {
        var particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem ps = particleSystems[i];
            if (ps == null) { continue; }
            var emission = ps.emission;
            emission.enabled = false;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        bool anyAlive = true;
        while (anyAlive)
        {
            anyAlive = false;
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem ps = particleSystems[i];
                if (ps != null && ps.IsAlive(true))
                {
                    anyAlive = true;
                    break;
                }
            }
            yield return null;
        }

        root.SetActive(false);
        _ultimateEffectOffRoutine = null;
    }

    /// <summary>
    /// 대쉬시 잔상 토글
    /// </summary>
    /// <param name="isOn"></param>
    public void RPC_SetGhostTrail(bool isOn)
    {
        if (!PhotonView.IsMine)
        {
            return;
        }
        PhotonView.RPC(nameof(SetGhostTrail), RpcTarget.All, isOn);
    }

    [PunRPC]
    public void SetGhostTrail(bool isOn)
    {
        GhostTrail trail = GetComponent<GhostTrail>();
        if (trail != null)
        {
            if (isOn)
            {
                trail.enabled = true;
            }
            else
            {
                // sequentially turn off, then disable component
                trail.TurnOffSequentiallyThenDisable();
            }
        }
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
        if (!PhotonView.IsMine)
        {
            return;
        }
        PhotonView.RPC(nameof(SetMaterial), RpcTarget.All, id);
    }

    public void ExecuteUltimate()
    {
        if (_playerStat.HasUltimateChance && !_playerStat.HasUsedUltimateThisLife)
        {
            if (_ultimate == null)
            {
                Debug.LogError("궁극기 스크립트를 찾을 수 없습니다.");
                return;
            }

            // 피가 부족하면 궁극기 사용 불가
            if (_playerStat.CurrentPlayerGunPowderCount <= _ultimate.GetCost())
            {
                return;
            }

            _ultimate.ExcuteUltimate();
            _playerStat.HasUsedUltimateThisLife = true;
            _playerStat.HasUltimateChance = false;
            _ultimateChanceTimer = 0f;
            RPC_UltimateEffect(false);
            int ultimateCost = _ultimate.GetCost();
            _playerStat.DecreaseGunPowderCount(ultimateCost, photonView.OwnerActorNr);
            RPC_SetMaterial((byte)EPlayerMaterial.Default);
            _ultimateEffectOn = false;

            // SFX
            _playerSFXAnimationEvent.PlayerUltimateUseSFX();

            // 궁극기 연출
            PhotonView.RPC(nameof(Rpc_UltimateProduction), RpcTarget.All, _ultimate.GetBombID());
        }
    }

    [PunRPC]
    public void Rpc_UltimateProduction(string bomb ,PhotonMessageInfo info)
    {
        PhotonPlayer player = info.Sender;
        EventManager.Instance.Ultimate(bomb, player);
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
            _colorUpdateWithoutAttackTimer = 0f;
            StopPreExplosionPulse(true);
            _warningSfxTimer = 0f;
            //PhotonView.RPC(nameof(DecreaseGunPowder), RpcTarget.All, PlayerStat.AttackPenaltyAmount);

            // 낙사 상태면 건파우더 감소 안함
            if (_playerStat.IsFallingDead)
            {
                return;
            }

            _playerStat.DecreaseGunPowderCount(PlayerStat.AttackPenaltyAmount, photonView.OwnerActorNr);

            RPC_ReleaseGunPowder(transform.position, PhotonView.OwnerActorNr, NO_ATTACK_RELEASE_COUNT, NO_ATTACK_RELEASE_SPREAD_ANGLE, NO_ATTACK_RELEASE_DISTANCE, true);
            if (PhotonView.IsMine && ExplosionEffectPrefab != null)
            {
                PhotonView.RPC(nameof(PlayExplosionEffect), RpcTarget.All);
            }

            ResetColorAndEffects();

            _playerFSM.SyncStateChange<PlayerDamagedState>();
        }
        else
        {
            bool flowControl = SetRedColorWithoutAttack();
            if (!flowControl)
            {
                return;
            }
        }
    }

    private void SetSpriteRendererWhite()
    {
        foreach (var renderer in _playerStat.MySpriteREndererList)
        {
            if (renderer == null) { continue; }
            renderer.color = Color.white;
        }
    }


    private bool SetRedColorWithoutAttack()
    {
        // 0.5초 간격으로만 색 업데이트
        _colorUpdateWithoutAttackTimer += Time.deltaTime;
        if (_colorUpdateWithoutAttackTimer < COLOR_UPDATE_TICK_SECONDS)
        {
            return false;
        }
        _colorUpdateWithoutAttackTimer = 0f;

        // 색 변화는 ratio 0.4부터 적용
        float ratio = _gunPowderDecreaseWithoutAttackTimer / PlayerStat.AttackPenaltyTime;
        if (ratio < REDNESS_START_RATIO)
        {
            // 원본 색상으로 복구
            RestoreOriginalColors();
            return true;
        }

        // 비율에 따라 펄스 시작/정지 (경고 단계)
        if (ratio >= WARNING_RATIO_THRESHOLD) { PlayPreExplosionPulse(); } else { StopPreExplosionPulse(false); }

        // ratio 0.4~1 -> S: 0~0.8로 맵핑 (H=0 고정, V는 유지)
        float t = Mathf.Clamp01((ratio - REDNESS_START_RATIO) / (1f - REDNESS_START_RATIO));
        float targetS = Mathf.Lerp(0f, MAX_RED_SATURATION, t);
        
        // 원본 색상을 기반으로 빨간색 적용
        if (_originalColorMap != null)
        {
            foreach (var kv in _originalColorMap)
            {
                if (kv.Key == null) { continue; }
                Color originalColor = kv.Value;
                Color.RGBToHSV(originalColor, out float _, out float _, out float v);
                Color newColor = Color.HSVToRGB(0f, targetS, v);
                newColor.a = originalColor.a;
                kv.Key.color = newColor;
            }
        }

        return true;
    }

    private void CheckAndPlayPreExplosionPulse()
    {
        float ratio = _gunPowderDecreaseWithoutAttackTimer / PlayerStat.AttackPenaltyTime;
        if (ratio >= WARNING_RATIO_THRESHOLD)
        {
            PlayPreExplosionPulse();
        }
        else
        {
            StopPreExplosionPulse(false);
        }
    }

    // 경고음: ratio가 0.7 이상일 때 점점 빠른 간격으로 재생
    private void UpdateWarningSfx()
    {
        if (_playerSFXAnimationEvent == null)
        {
            return;
        }
        float ratio = _gunPowderDecreaseWithoutAttackTimer / PlayerStat.AttackPenaltyTime;
        if (ratio < WARNING_RATIO_THRESHOLD)
        {
            _warningSfxTimer = 0f;
            return;
        }

        // WARNING_RATIO_THRESHOLD → 1.0 사이에서 재생 간격을 선형으로 WARNING_INTERVAL_MAX → WARNING_INTERVAL_MIN로 축소, 피치 WARNING_PITCH_MIN → WARNING_PITCH_MAX로 상승
        float t = Mathf.InverseLerp(WARNING_RATIO_THRESHOLD, 1f, Mathf.Clamp01(ratio));
        float interval = Mathf.Lerp(WARNING_INTERVAL_MAX, WARNING_INTERVAL_MIN, t);
        float pitch = Mathf.Lerp(WARNING_PITCH_MIN, WARNING_PITCH_MAX, t);
        _warningSfxTimer += Time.deltaTime;
        if (_warningSfxTimer >= interval)
        {
            _warningSfxTimer = 0f;
            // 로컬 소유자만 재생
            if (PhotonView.IsMine)
            {
                _playerSFXAnimationEvent.PlayerWithoutAttackSFX(pitch);
            }
        }
    }

    private void PlayPreExplosionPulse()
    {
        if (_preExplosionPulseTween != null && _preExplosionPulseTween.IsActive())
        {
            return;
        }

        float targetScaleMultiplier = PULSE_SCALE_MULTIPLIER;
        float halfDuration = PULSE_HALF_DURATION; // 커졌다/작아졌다 왕복 0.4초
        transform.localScale = _defaultLocalScale;

        Sequence seq = DOTween.Sequence();
        // 커질 때 빨강으로 (원본 색상 기반)
        seq.AppendCallback(() =>
        {
            if (_originalColorMap != null)
            {
                foreach (var kv in _originalColorMap)
                {
                    if (kv.Key == null) { continue; }
                    Color originalColor = kv.Value;
                    Color.RGBToHSV(originalColor, out float _, out float _, out float v);
                    Color redCol = Color.HSVToRGB(0f, MAX_RED_SATURATION, v);
                    redCol.a = originalColor.a;
                    kv.Key.color = redCol;
                }
            }
        });
        seq.Append(transform.DOScale(_defaultLocalScale * targetScaleMultiplier, halfDuration).SetEase(Ease.InOutSine));
        // 작아질 때 원본 색으로 복구
        seq.AppendCallback(() =>
        {
            RestoreOriginalColors();
        });
        seq.Append(transform.DOScale(_defaultLocalScale, halfDuration).SetEase(Ease.InOutSine));
        seq.SetLoops(-1, LoopType.Restart);
        _preExplosionPulseTween = seq;
    }

    private void StopPreExplosionPulse(bool resetScale)
    {
        if (_preExplosionPulseTween != null)
        {
            _preExplosionPulseTween.Kill(false);
            _preExplosionPulseTween = null;
        }
        
        // 원본 색상으로 복구
        RestoreOriginalColors();
        
        if (resetScale)
        {
            transform.localScale = _defaultLocalScale;
        }
    }

    /// <summary>
    /// 저장된 원본 색상으로 스프라이트를 복구합니다.
    /// </summary>
    private void RestoreOriginalColors()
    {
        if (_originalColorMap != null && _originalColorMap.Count > 0)
        {
            foreach (var kv in _originalColorMap)
            {
                if (kv.Key != null)
                {
                    kv.Key.color = kv.Value;
                }
            }
        }
        else
        {
            // 원본 색상 정보가 없는 경우 흰색으로 설정
            SetSpriteRendererWhite();
        }
    }

    [PunRPC]
    private void PlayExplosionEffect()
    {
        VFXPool.Instance.Play(ExplosionEffectPrefab.name, transform.position);
    }

    // HitEffect 네트워크 동기화 메서드들 추가
    public void RPC_SetHitEffect(bool isActive)
    {
        if (!PhotonView.IsMine)
        {
            return;
        }
        PhotonView.RPC(nameof(SetHitEffect), RpcTarget.All, isActive);
    }

    [PunRPC]
    public void SetHitEffect(bool isActive)
    {
        if (HitEffectPrefab != null)
        {
            HitEffectPrefab.SetActive(isActive);
        }
    }

    public void RPC_PlayFallDeadVFX()
    {
        if (!PhotonView.IsMine)
        {
            return;
        }
        PhotonView.RPC(nameof(PlayFallDeadVFX), RpcTarget.All);
    }

    [PunRPC]
    public void PlayFallDeadVFX()
    {
        if (FallDeadVFXPrefab != null)
        {
            VFXPool.Instance.Play(FallDeadVFXPrefab.name, transform.position);
            _playerSFXAnimationEvent.PlayerFallDeadExplosionSFX();
        }
    }

    public void RPC_PlayFallDeadExplosionVFX()
    {
        if (!PhotonView.IsMine)
        {
            return;
        }
        PhotonView.RPC(nameof(PlayFallDeadExplosionVFX), RpcTarget.All);
    }

    [PunRPC]
    public void PlayFallDeadExplosionVFX()
    {
        if (ExplosionEffectPrefab != null)
        {
            VFXPool.Instance.Play(ExplosionEffectPrefab.name, transform.position);
        }
    }


    /// <summary>
    /// 색상과 시각적 효과를 모두 초기화하는 메서드
    /// </summary>
    private void ResetColorAndEffects()
    {
        // 펄스 효과 중단 및 스케일 리셋
        StopPreExplosionPulse(true);
        
        // 스프라이트 색상을 원본 색상으로 초기화
        RestoreOriginalColors();
        
        // 타이머들 초기화
        _gunPowderDecreaseWithoutAttackTimer = 0f;
        _colorUpdateWithoutAttackTimer = 0f;
        _warningSfxTimer = 0f;
    }

    /// <summary>
    /// 공격을 하면 타이머 초기화
    /// </summary>
    public void ResetGunPowderDecreaseWithoutAttackTimer()
    {
        ResetColorAndEffects();
    }

    public void PlayerTeamCheck()
    {

    }

    public void TakeDamage(int damage, int maxDamage, int HealPercent, Vector3 attackerBomb, int attackerViewId, int attackerActorNumber, bool isFallingOut)
    {
        Debug.Log("TakeDamage");
        EventManager.Instance.HitScreen();
        // 피격 VFX 재생
        if (tag == "Player")
        {
            VFXPool.Instance.RandomPlay("Damaged", transform.position, 1, 3);
        }
        else
        {
            VFXPool.Instance.RandomPlay("Hit", transform.position, 1, 6);
        }
        
        // SFX

        // 맥스 데미지를 받았을때 다른 사운드 재생
        if( damage == maxDamage)
        {
            _playerSFXAnimationEvent.PlayerCritDamageVoiceRandomSFX();
            SoundManager.Instance.PlayLocalRandomSound("PlayerDamage", transform, 1, 7, 0f, false, SoundType.SFX, true, 1f, 50f);
        }
        else
        {
            SoundManager.Instance.PlayLocalRandomSound("PlayerDamage", transform, 1, 7, 0f, false, SoundType.SFX, true, 1f, 50f);
            SoundManager.Instance.PlayLocalRandomSound("PlayerDamageVoice", transform, 1, 3, 0f, false, SoundType.SFX, true, 1f, 50f);
        }

        if (!PhotonView.IsMine)
        {
            return;
        }
        PhotonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damage, maxDamage, HealPercent, attackerBomb, attackerViewId, attackerActorNumber, isFallingOut);
    }

    [PunRPC]
    public void RPC_TakeDamage(int damage, int maxDamage, int HealPercent, Vector3 attackerBomb, int attackerViewId, int attackerActorNumber, bool isFallingOut, PhotonMessageInfo info)
    {
        if (_playerStat.IsImmune)
        {
            return;
        }

        // 공격자 정보 가져오기
        PhotonView attackerView = PhotonView.Find(attackerViewId);
        if (attackerView != null && attackerView.gameObject != null && attackerView.gameObject.activeInHierarchy)
        {
            PlayerStat attackerStat = attackerView.GetComponent<PlayerStat>();

            // 팀 비교는 동기화된 로컬 필드 사용 (시작 시 CustomProperties로부터 동기화됨)
            EInGameTeam attackerTeam = attackerStat != null ? attackerStat.Team : EInGameTeam.Default;
            EInGameTeam victimTeam = _playerStat.Team;

            // 팀 체크: 같은 팀이면서 자기 자신이 아닌 경우 데미지 무시
            if (attackerTeam == victimTeam && attackerActorNumber != PhotonView.OwnerActorNr)
            {
                // 같은 팀이므로 데미지 적용하지 않음 (VFX, 사운드 등은 그대로 재생)
                return;
            }
        }
        else
        {
            Debug.LogWarning($"[RPC_TakeDamage] 공격자 뷰를 찾을 수 없습니다. ID: {attackerViewId}");
        }

        // 피격 횟수 증가
        _playerStat.IncreseDamagedCount();

        // 건파우더 드랍량 계산 (힐량 계산)
        float healPercent = HealPercent / 100f;
        int gunPowderCount = Mathf.CeilToInt(maxDamage * healPercent);

        // 플레이어가 맞은 횟수에 비례해서 데미지 증가
        int increaseDamagePerDamagedCount = _playerStat.CurrentPlayerDamagedCount / 15;
        damage += increaseDamagePerDamagedCount;
        maxDamage += increaseDamagePerDamagedCount;

        // 체력 감소
        bool isDead = _playerStat.DecreaseGunPowderCount(damage, attackerActorNumber);

        // 날 때린 사람 딜량 증가
        if (attackerView != null && attackerView.gameObject != null && attackerView.gameObject.activeInHierarchy)
        {
            PlayerStat attackerStat = attackerView.GetComponent<PlayerStat>();
            if (attackerStat != null)
            {
                attackerStat.IncreaseTotalDamage(damage);
                /*
                if (isDead)
                {
                    // 킬 카운트는 공격자 본인의 클라이언트에서만 증가시키도록 RPC 호출
                    if (attackerView.Owner != null)
                    {
                        attackerView.RPC(nameof(PlayerStat.RPC_IncreaseTotalKillCount), attackerView.Owner);
                    }
                }*/
            }
        }

        // Gunpowder 낙출
        ReleaseGunPowder(attackerBomb, attackerViewId, gunPowderCount, _gunPowderSpreadAngle, _gunPowderSpreadDistance, isFallingOut);

        // 피격 이벤트 발생
        OnHit?.Invoke();

        // 데미지 팝업: 중복 호출 방지
        // 오직 RPC_TakeDamage를 원래 보낸 클라이언트(피격자 Owner)에서만 팝업 RPC를 전송한다
        if (info.Sender != null && info.Sender.IsLocal)
        {
            // 맞은 사람(Owner)에게는 -damage 표시
            if (PhotonView.Owner != null)
            {
                PhotonView.RPC(nameof(ShowDamagePopup), PhotonView.Owner, -damage, maxDamage);
            }
            // 때린 사람(Attacker Owner)에게는 +damage 표시 (피해자와 동일 Owner면 중복 방지)
            if (attackerView != null && attackerView.gameObject != null && attackerView.gameObject.activeInHierarchy && attackerView.Owner != null && attackerView.Owner != PhotonView.Owner)
            {
                PhotonView.RPC(nameof(ShowDamagePopup), attackerView.Owner, damage, maxDamage);
            }
        }
    }

    /// <summary>
    /// 피격시 건파우더 흩뿌리기
    /// </summary>
    [PunRPC]
    public void ReleaseGunPowder(Vector3 explosionOrigin, int attackerViewId, int count = 3, float spreadAngle = 30f,
     float distance = 1.0f, bool isFallingOut = true)
    {
        if (!PhotonNetwork.IsMasterClient)
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
            int randomSeed = UnityEngine.Random.Range(0, 9999);


            object[] instData = new object[] { attackerViewId, isFallingOut, randomSeed, PhotonView.ViewID };
            PhotonNetwork.Instantiate(GunPowderPrefab.name, spawnPos, Quaternion.identity, 0, instData);
        }
    }

    public void RPC_ReleaseGunPowder(Vector3 explosionOrigin, int attackerViewId, int count = 3, float spreadAngle = 30f,
     float distance = 1.0f, bool isFallingOut = true)
    {
        if (!PhotonView.IsMine)
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
    public (Transform transform, EBombSpawnPoint point) GetBombSpawnInfo()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        EBombSpawnPoint point;
        switch ((h, v))
        {
            case (1, 0): point = EBombSpawnPoint.Right; break;
            case (1, 1): point = EBombSpawnPoint.RightUp; break;
            case (0, 1): point = EBombSpawnPoint.Up; break;
            case (-1, 1): point = EBombSpawnPoint.LeftUp; break;
            case (-1, 0): point = EBombSpawnPoint.Left; break;
            case (-1, -1): point = EBombSpawnPoint.LeftDown; break;
            case (0, -1): point = EBombSpawnPoint.Down; break;
            case (1, -1): point = EBombSpawnPoint.RightDown; break;
            default:
                point = _playerStat.FacingDirection == 1 ? EBombSpawnPoint.Right : EBombSpawnPoint.Left; break;
        }
        return (_bombSpawnPointList[(int)point], point);
    }

    public Transform GetBombSpawnPoint()
    {
        return GetBombSpawnInfo().transform;
    }

    public (Transform transform, EBombSpawnPoint point) GetExplosionSpawnInfo()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        EBombSpawnPoint point;
        switch ((h, v))
        {
            case (-1, 0): point = EBombSpawnPoint.Right; break;
            case (-1, -1): point = EBombSpawnPoint.RightUp; break;
            case (0, -1): point = EBombSpawnPoint.Up; break;
            case (1, -1): point = EBombSpawnPoint.LeftUp; break;
            case (1, 0): point = EBombSpawnPoint.Left; break;
            case (1, 1): point = EBombSpawnPoint.LeftDown; break;
            case (0, 1): point = EBombSpawnPoint.Down; break;
            case (-1, 1): point = EBombSpawnPoint.RightDown; break;
            default: point = EBombSpawnPoint.Down; break;
        }
        return (_explosionSpawnPointList[(int)point], point);
    }

    public Transform GetExplosionSpawnPoint()
    {
        return GetExplosionSpawnInfo().transform;
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
        if (!PhotonView.IsMine)
        {
            return;
        }

        PhotonView.RPC(nameof(SetAnimatorTrigger), RpcTarget.All, triggerName);
    }

    [PunRPC]
    public void RPC_SetAnimatorBool(string boolName, bool value)
    {
        if (!PhotonView.IsMine)
        {
            return;
        }

        PhotonView.RPC(nameof(SetAnimatorBool), RpcTarget.All, boolName, value);
    }

    [PunRPC]
    public void ResetAnimatorTrigger(string triggerName)
    {
        foreach (Animator animator in _myAnimatorList)
        {
            animator.ResetTrigger(triggerName);
        }
    }

    [PunRPC]
    public void RPC_ResetAnimatorTrigger(string triggerName)
    {
        if (!PhotonView.IsMine)
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
            if (fsmForGuard.IsCurrentState<PlayerDieState>()
            && stateName != nameof(PlayerIdleState)
            && stateName != nameof(PlayerObserveState))
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
                case "PlayerObserveState":
                    playerFSM.ChangeState<PlayerObserveState>();
                    break;
                case "PlayerConfuseState":
                    playerFSM.ChangeState<PlayerConfuseState>();
                    break;
                default:
                    break;
            }
        }
        else
        {
            Debug.LogError("PlayerFSM 컴포넌트를 찾을 수 없습니다!");
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

    public void SetAirDropItem(AirDropItemBase airDropItem)
    {
        if (PhotonView.IsMine)
        {
            AirDropItemLootVFX.StartRoulette(airDropItem);
        }
        _airDropItem = airDropItem;
    }

    public void RemoveAirDropItem()
    {
        _airDropItem = null;
    }

    [PunRPC]
    public void RPC_SetIsImmune(bool isImmune)
    {
        _playerStat.IsImmune = isImmune;
    }

    public void SetDownJump()
    {
        gameObject.layer = LayerMask.NameToLayer("DownJump");
        // 하위 오브젝트 들도 모드 변경
        foreach (Transform child in transform)
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
        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }

    public void RPC_HeadSpriteOnOff()
    {
        if (!PhotonView.IsMine)
        {
            return;
        }
        PhotonView.RPC(nameof(HeadSpriteOnOff), RpcTarget.All);
    }

    [PunRPC]
    private void HeadSpriteOnOff()
    {
        StartCoroutine(HeadSpriteOnOffCoroutine());
    }

    private IEnumerator HeadSpriteOnOffCoroutine()
    {
        _playerStat.MySpriteREndererList[0].enabled = false;
        _playerStat.MySpriteREndererList[2].enabled = false;
        _playerStat.MySpriteREndererList[3].enabled = false;

        yield return new WaitForSeconds(BasicBombStat.CoolTime);

        _playerStat.MySpriteREndererList[0].enabled = true;
        _playerStat.MySpriteREndererList[2].enabled = true;
        _playerStat.MySpriteREndererList[3].enabled = true;
    }

    public void Confuse()
    {
        _playerFSM.ChangeState<PlayerConfuseState>();
    }

    public void SetPlayerWet()
    {
        _playerStat.SetWetState();
    }

    public void ResetPlayerWet()
    {
        _playerStat.ResetWetState();
    }

    [PunRPC]
    public void ShowDamagePopup(int value, int maxDamage)
    {
        if (_damagePopup == null)
        {
            _damagePopup = GetComponent<DamagePopup>();
        }
        if (_damagePopup == null)
        {
            return;
        }
        _damagePopup.SpawnPopup(value, maxDamage);
    }
}
