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

    // 타이머는 PlayerGunpowderController로 이동
    public float AttackTimer => _gunpowderController != null ? _gunpowderController.AttackTimer : 0f;
    public float GunPowderDecreaseTimer => _gunpowderController != null ? _gunpowderController.GunPowderDecreaseTimer : 0f;
    public float GunPowderDecreaseWithoutAttackTimer => _gunpowderController != null ? _gunpowderController.GunPowderDecreaseWithoutAttackTimer : 0f;

    private Tween _preExplosionPulseTween;
    private Vector3 _defaultLocalScale;
    private Dictionary<SpriteRenderer, Color> _originalColorMap; // 게임 시작 시 저장되는 진짜 원본 색상
    private Dictionary<SpriteRenderer, int> _originalSortingOrderMap; // 스프라이트 렌더러의 원본 sortingOrder 저장
    private bool _isColorRestored = true; // 색상이 원본 상태인지 추적
    private readonly List<SpriteRenderer> _dieSpriteRendererList = new List<SpriteRenderer>();
    public IReadOnlyList<SpriteRenderer> DieSpriteRendererList => _dieSpriteRendererList;

    [Header("히트스탑")]
    [SerializeField]
    private Vector2 _storedVelocity = Vector2.zero;
    public Vector2 StoredVelocity => _storedVelocity;
    [SerializeField]
    private bool _hasStoredVelocity = false;
    public bool HasStoredVelocity => _hasStoredVelocity;

    public event Action OnAttack;
    public event Action OnHit;
    public event Action OnNormalAttack;
    public event Action OnSpecialAttack;
    public event Action OnUltimateChanceActivated;   // 궁극기 사용 가능 상태 활성화
    public event Action OnUltimateChanceDeactivated; // 궁극기 사용 가능 상태 비활성화

    private PlayerUltimateController _ultimateController;
    public PlayerUltimateController UltimateController => _ultimateController;

    private PlayerGunpowderController _gunpowderController;
    public PlayerGunpowderController GunpowderController => _gunpowderController;

    [SerializeField]
    private BoxRay2D _groundRay2D;
    public BoxRay2D GroundRay2D => _groundRay2D;

    [Header("프리팹 참조")]
    public GameObject HeadBombPrefab;
    public GameObject GunPowderPrefab;
    public GameObject DieExplosionPrefab;
    public GameObject HitEffectPrefab;
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

    public Ultimate Ultimate => _ultimateController != null ? _ultimateController.Ultimate : null;

    private PlayerMaterial _playerMaterial;
    private PlayerFSM _playerFSM;
    public PlayerFSM PlayerFSM => _playerFSM;
    private PlayerDamageController _damageController;
    public PlayerDamageController DamageController => _damageController;
    private DamagePopup _damagePopup;
    public DamagePopup DamagePopup => _damagePopup;
    private IPlayerSkinManager _skinManager;


    private PlayerBuffHandler _playerBuffHandler;
    public PlayerBuffHandler PlayerBuffHandler => _playerBuffHandler;

    public bool IsSuperArmor = false;
    private RigidbodyConstraints2D _originalConstraints; // SuperArmor 적용 전 원본 제약 조건


    [SerializeField]
    private PlayerSFXAnimationEvent _playerSFXAnimationEvent;
    public PlayerSFXAnimationEvent PlayerSFXAnimationEvent => _playerSFXAnimationEvent;

    [Header("스킨")]

    [Header("크랩용")]
    public Transform CrabHoldPoint;

    // private PlayerHealthBar _playerHealthBar;
    // public PlayerHealthBar PlayerHealthBar => _playerHealthBar;

    // 대시 탭 타임
    public float LastDashTapTimeLeft = -999f;
    public float LastDashTapTimeRight = -999f;

    public float UltimateChanceTimer { get => _ultimateController != null ? _ultimateController.UltimateChanceTimer : 0f; set { if (_ultimateController != null) _ultimateController.UltimateChanceTimer = value; } }

    // 공격 없을 때 경고 상태는 PlayerGunpowderController로 이동

    // 최근 피격 데미지 비율 (거리 기반 넉백 효과를 위해)
    private float _lastDamageRatio = 1f; // damage / maxDamage 비율
    public float LastDamageRatio => _lastDamageRatio;

    // 부활 후 첫 공격 여부 (HP bar 최대값 리셋용)
    private bool _isAfterResurrect = false;

    [Header("PlayerHitParticle")]
    [SerializeField] private GameObject _playerHitPrefab;           // 자신의 우측에 생성
    [SerializeField] private GameObject _playerHitParticlePrefab;  // 상대방 위치에 생성
    [SerializeField] private GameObject _playerCritHitPrefab;       // 자신의 우측에 생성 (크리티컬)
    [SerializeField] private GameObject _playerCritHitParticlePrefab;  // 상대방 위치에 생성 (크리티컬)
    [SerializeField] private GameObject _playerGunPowderUsePrefab;     // 건파우더 사용 파티클
    [SerializeField] private Vector3 _playerHitVFXOffset = new Vector3(0, 5f, 0f); // 히트 파티클 오프셋 (위쪽으로 5f만큼 올림)
    private const float PLAYER_HIT_PARTICLE_OFFSET = 0.5f;  // 자신의 우측 오프셋
    public GameObject PlayerHitPrefab => _playerHitPrefab;
    public GameObject PlayerHitParticlePrefab => _playerHitParticlePrefab;
    public GameObject PlayerCritHitPrefab => _playerCritHitPrefab;
    public GameObject PlayerCritHitParticlePrefab => _playerCritHitParticlePrefab;
    public Vector3 PlayerHitVFXOffset => _playerHitVFXOffset;
    public float PlayerHitParticleOffset => PLAYER_HIT_PARTICLE_OFFSET;

    // 폭발당 히트 파티클 수집용
    private class PendingExplosionHit
    {
        public List<Vector3> victimPositions = new List<Vector3>();
        public List<int> victimViewIds = new List<int>(); // 피격자 ViewID (FollowVFX용)
        public List<bool> victimIsCrits = new List<bool>(); // 각 피격자의 크리티컬 여부 (개별 파티클용)
        public bool hasCrit = false; // 폭발에 크리티컬이 하나라도 있는지 (공격자 이펙트/사운드용)
    }

    private PendingExplosionHit _currentExplosionHit;
    private Coroutine _processHitCoroutine;

    private void Awake()
    {
        _playerStat = GetComponent<PlayerStat>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        PhotonView = GetComponent<PhotonView>();
        _playerMaterial = GetComponent<PlayerMaterial>();
        _playerFSM = GetComponent<PlayerFSM>();
        _damageController = GetComponent<PlayerDamageController>();
        _damagePopup = GetComponent<DamagePopup>();
        _skinManager = GetComponent<PlayerSkinManager>();
        _ultimateController = GetComponent<PlayerUltimateController>();
        _gunpowderController = GetComponent<PlayerGunpowderController>();
        // _playerHealthBar = GetComponentInChildren<PlayerHealthBar>();

        EquipedItemDict = new Dictionary<EItemType, ItemDTO>();
        _originalSortingOrderMap = new Dictionary<SpriteRenderer, int>();

        _playerBuffHandler = GetComponent<PlayerBuffHandler>();
        IsSuperArmor = false;

        // 원본 Rigidbody constraints 저장
        if (_rigidbody2D != null)
        {
            _originalConstraints = _rigidbody2D.constraints;
        }


        // 기본 폭탄 정보 가져오기
        GameObject basicBomb = ItemDatabase.Instance.GetItem(BASIC_BOMB_ID).Prefab;
        _normalBomb = basicBomb.GetComponent<Bomb>();
        BasicBombStat = ItemDatabase.Instance.GetStat<BombStat>(BASIC_BOMB_ID);

        if (UI_PingBase.Instance != null)
        {
            UI_PingBase.Instance.SetPing(this.transform);
        }

        UnityEngine.Random.InitState(RANDOM_SEED);

        InitializeBodyParts();
    }

    private void OnDisable()
    {
        // DOTween 정리 (Kill 시 OnKill 콜백에서 RestoreOriginalColors 자동 호출됨)
        if (_preExplosionPulseTween != null)
        {
            _preExplosionPulseTween.Kill(false);
            _preExplosionPulseTween = null;
        }
        else
        {
            // Tween이 없는 경우에만 직접 복원
            RestoreOriginalColors();
        }
    }

    private void OnDestroy()
    {
        // OnDisable에서 이미 색상 복원이 처리되므로 여기서는 제거

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

        // DOTween 정리
        if (_preExplosionPulseTween != null)
        {
            _preExplosionPulseTween.Kill(false);
            _preExplosionPulseTween = null;
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

        InitializeDieSpriteRenderers();
    }

    private void InitializeDieSpriteRenderers()
    {
        _dieSpriteRendererList.Clear();
        foreach (var part in _diePartList)
        {
            if (part == null) continue;

            SpriteRenderer[] spriteRenderers = part.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var spriteRenderer in spriteRenderers)
            {
                RegisterDieSpriteRenderer(spriteRenderer);
            }
        }
    }

    public void RegisterDieSpriteRenderer(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null || _dieSpriteRendererList.Contains(spriteRenderer))
        {
            return;
        }

        _dieSpriteRendererList.Add(spriteRenderer);
        RegisterOriginalColor(spriteRenderer);
        RegisterOriginalSortingOrder(spriteRenderer);
    }

    public void UnregisterDieSpriteRenderer(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (_dieSpriteRendererList.Contains(spriteRenderer))
        {
            _dieSpriteRendererList.Remove(spriteRenderer);
        }

        UnregisterOriginalColor(spriteRenderer);
        UnregisterOriginalSortingOrder(spriteRenderer);
    }

    [PunRPC]
    private void RPC_LoadItems()
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
            else
            {
                // 해당 슬롯이 해제되었거나 값이 제거된 경우 로컬 딕셔너리에서도 제거
                if (EquipedItemDict.ContainsKey(itemType))
                {
                    EquipedItemDict.Remove(itemType);
                }
            }
        }

        // [스킨] 단순 존재 여부 기반 적용/해제: 장착되었으면 적용, 없으면 해제
        ItemDTO headItem = null;
        ItemDTO faceItem = null;
        ItemDTO chestItem = null;
        ItemDTO capeItem = null;

        EquipedItemDict.TryGetValue(EItemType.Head, out headItem);
        EquipedItemDict.TryGetValue(EItemType.Face, out faceItem);
        EquipedItemDict.TryGetValue(EItemType.Chest, out chestItem);
        EquipedItemDict.TryGetValue(EItemType.Cape, out capeItem);

        if (headItem != null) { ApplyHeadSkin(headItem); } else { ClearHeadSkin(); }
        if (faceItem != null) { ApplyFaceSkin(faceItem); } else { ClearFaceSkin(); }
        if (chestItem != null) { ApplyChestSkin(chestItem); } else { ClearChestSkin(); }
        if (capeItem != null) { ApplyCapeSkin(capeItem); } else { ClearCapeSkin(); }

        SpriteFlipx();

        // 특수폭탄 정보 받아오기                
        SpecialBombStat = ItemDatabase.Instance.GetStat<BombStat>(EquipedItemDict[EItemType.Bomb].ID);

        // 궁극기 설정
        if (UltimateManager.Instance != null && _ultimateController != null)
        {
            Ultimate ultimate = UltimateManager.Instance.GetUltimate(EquipedItemDict[EItemType.Bomb].ID, this);
            _ultimateController.SetUltimate(ultimate);
        }

        SetPlayerOrderInLayer();
    }

    private void SetPlayerOrderInLayer()
    {
        int playerOrderInLayerPlus = PhotonView.OwnerActorNr;
        foreach (var item in _playerStat.MySpriteREndererList)
        {
            if (item != null)
            {
                // 원본 sortingOrder를 저장하고 있지 않다면 현재 값을 원본으로 저장
                if (!_originalSortingOrderMap.ContainsKey(item))
                {
                    _originalSortingOrderMap[item] = item.sortingOrder;
                }

                // 원본 값에 플레이어 오프셋을 더해서 설정
                item.sortingOrder = _originalSortingOrderMap[item] + playerOrderInLayerPlus * 100;
            }
        }
    }

    public void LoadItems()
    {
        if (!PhotonView.IsMine)
        {
            return;
        }

        PhotonView.RPC(nameof(RPC_LoadItems), RpcTarget.All);
    }

    /// <summary>
    /// 스킨 적용/해제 핸들러
    /// </summary>
    /// <param name="item"></param>
    private void ApplyHeadSkin(ItemDTO item)
    {
        if (_skinManager != null) { _skinManager.ApplyHead(item); }
    }
    private void ClearHeadSkin()
    {
        if (_skinManager != null) { _skinManager.ClearHead(); }
    }
    private void ApplyFaceSkin(ItemDTO item)
    {
        if (_skinManager != null) { _skinManager.ApplyFace(item); }
    }
    private void ClearFaceSkin()
    {
        if (_skinManager != null) { _skinManager.ClearFace(); }
    }
    private void ApplyChestSkin(ItemDTO item)
    {
        if (_skinManager != null) { _skinManager.ApplyChest(item); }
    }
    private void ClearChestSkin()
    {
        if (_skinManager != null) { _skinManager.ClearChest(); }
    }
    private void ApplyCapeSkin(ItemDTO item)
    {
        if (_skinManager != null) { _skinManager.ApplyCape(item); }
    }
    private void ClearCapeSkin()
    {
        if (_skinManager != null) { _skinManager.ClearCape(); }
    }

    // Player는 스킨 내부 구현을 가지지 않도록 정리 (외부 매니저로 위임)

    // 내부 구현 제거됨 (스킨 관리는 PlayerSkinManager에서 처리)

    private void Start()
    {
        // 기본 스프라이트 렌더러들의 원본 sortingOrder 저장
        InitializeOriginalSortingOrders();

        LoadItems();

        // 1. 이벤트 핸들러 등록
        _playerStat.OnGunPowderEmpty += HandleGunPowderEmpty;
        _playerStat.OnGunpowderIncreased += HandleGunpowderIncreased;
        EventManager.Instance.OnPlayerItemChanged += LoadItems;

        // 궁극기 컨트롤러 이벤트 연결
        if (_ultimateController != null)
        {
            _ultimateController.OnUltimateChanceActivated += () => OnUltimateChanceActivated?.Invoke();
            _ultimateController.OnUltimateChanceDeactivated += () => OnUltimateChanceDeactivated?.Invoke();
        }

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
        if (_gunpowderController != null)
        {
            _gunpowderController.ResetTimers();
        }
        if (_ultimateController != null)
        {
            _ultimateController.ResetUltimateChanceTimer();
        }

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
        SyncTeamFromCustomProperties();

        // 모든 플레이어의 팀 동기화 (지연 호출로 CustomProperties 동기화 대기)
        if (PhotonView.IsMine)
        {
            Invoke(nameof(SyncAllPlayersTeam), 0.5f); // 0.5초 후 모든 플레이어의 팀 동기화
        }
    }

    void OnEnable()
    {
        if (_playerFSM != null)
        {
            _playerFSM.SyncStateChange<PlayerIdleState>();
        }

        // GameObject가 활성화될 때 팀 동기화 (뒤에 들어온 플레이어의 경우)
        // SyncTeamFromCustomProperties();
    }

    /// <summary>
    /// 게임 시작 시 원본 색상을 저장합니다. (한 번만 실행)
    /// 이 색상 정보는 절대 변경되지 않으며, 모든 색상 효과가 끝날 때 이 색상으로 복원됩니다.
    /// </summary>
    private void InitializeOriginalColors()
    {
        if (_originalColorMap == null)
        {
            _originalColorMap = new Dictionary<SpriteRenderer, Color>();
        }

        foreach (var renderer in _playerStat.MySpriteREndererList)
        {
            if (renderer != null && !_originalColorMap.ContainsKey(renderer))
            {
                // 원본 색상 저장 (이 값은 절대 변경되지 않음)
                _originalColorMap[renderer] = renderer.color;
            }
        }
    }

    /// <summary>
    /// 스킨 동적 추가 시 색상 시스템에 편입
    /// 주의: 스프라이트가 원본 색상 상태일 때 호출해야 합니다.
    /// </summary>
    public void RegisterOriginalColor(SpriteRenderer renderer)
    {
        if (renderer == null) { return; }
        if (_originalColorMap == null)
        {
            _originalColorMap = new Dictionary<SpriteRenderer, Color>();
        }
        if (!_originalColorMap.ContainsKey(renderer))
        {
            // 현재 색상이 빨간색(경고 상태)인지 확인
            Color currentColor = renderer.color;
            Color.RGBToHSV(currentColor, out float h, out float s, out float v);

            // H가 0이고 S가 높으면 빨간색으로 판단 -> 흰색으로 저장
            bool isRedWarning = (h < 0.05f || h > 0.95f) && s > 0.3f;

            if (isRedWarning)
            {
                // 빨간색 경고 상태이면 기본 색상(흰색)을 원본으로 저장
                _originalColorMap[renderer] = Color.white;
                // 실제 스프라이트도 흰색으로 즉시 변경
                renderer.color = Color.white;
            }
            else
            {
                // 정상 색상이면 현재 색상을 원본으로 저장
                _originalColorMap[renderer] = currentColor;
            }
        }
    }

    public void UnregisterOriginalColor(SpriteRenderer renderer)
    {
        if (renderer == null || _originalColorMap == null) { return; }
        if (_originalColorMap.ContainsKey(renderer))
        {
            _originalColorMap.Remove(renderer);
        }
    }

    // 스킨 동적 추가 시 sortingOrder 시스템에 편입/해제
    public void RegisterOriginalSortingOrder(SpriteRenderer renderer)
    {
        if (renderer == null) { return; }
        if (_originalSortingOrderMap == null)
        {
            _originalSortingOrderMap = new Dictionary<SpriteRenderer, int>();
        }
        if (!_originalSortingOrderMap.ContainsKey(renderer))
        {
            _originalSortingOrderMap[renderer] = renderer.sortingOrder;
        }
    }

    public void UnregisterOriginalSortingOrder(SpriteRenderer renderer)
    {
        if (renderer == null || _originalSortingOrderMap == null) { return; }
        if (_originalSortingOrderMap.ContainsKey(renderer))
        {
            _originalSortingOrderMap.Remove(renderer);
        }
    }

    // 플레이어 시작 시 기본 스프라이트 렌더러들의 원본 sortingOrder 저장
    private void InitializeOriginalSortingOrders()
    {
        if (_playerStat?.MySpriteREndererList == null) { return; }

        foreach (var renderer in _playerStat.MySpriteREndererList)
        {
            if (renderer != null)
            {
                RegisterOriginalSortingOrder(renderer);
            }
        }
    }

    public void ResurrectPlayer()
    {
        // 각종 타이머들 초기화
        if (_gunpowderController != null)
        {
            _gunpowderController.ResetTimers();
        }
        _lastNormalBombTime = 0f;
        _lastSpecialBombTime = 0f;
        if (_ultimateController != null)
        {
            _ultimateController.ResetUltimateChanceTimer();
        }

        // 저장된 속도 상태 초기화
        ClearStoredVelocity();

        // 경고 상태 완전 해제
        ClearNoAttackWarning();

        // 플레이어 스탯 초기화 (건파우더 초기화)
        _playerStat.ResurrectPlayerStat();

        // HP bar 초기화 (부활 시 maxHP를 초기값으로 리셋)
        // if (_playerHealthBar != null)
        // {
        //     _playerHealthBar.ResetHealthBarOnResurrect();
        // }

        // 부활 후 첫 공격 플래그 설정
        _isAfterResurrect = true;
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
        // 죽을 때 모든 효과 초기화 (경고 + 궁극기)
        // 순서: 깜박임 해제 → 궁극기 해제 → 색상 복원
        if (_ultimateController != null && _ultimateController.IsUltimateEffectOn())
        {
            // 궁극기 효과 비활성화 (내부에서 경고도 자동으로 해제됨)
            _ultimateController.SetUltimateEffectState(false);
        }
        else
        {
            // 궁극기가 없으면 경고만 해제
            ClearNoAttackWarning();
        }

        _playerStat.HasUltimateChance = false;
        if (_ultimateController != null)
        {
            _ultimateController.ResetUltimateChanceTimer();
        }

        if (_playerStat.CurrentPlayerLife > 0)
        {
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
                // 네트워크 동기화된 상태 변경
                _playerFSM.SyncStateChange<PlayerDieState>();
            }

            return;
        }

        if (photonView.IsMine == false)
        {
            return;
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
        {
            {EProperties.IsDead.ToString(), true},
            {EProperties.Kill.ToString(), PlayerStat.TotalKillCount},
            {EProperties.Damage.ToString(), PlayerStat.TotalDamage}
        });

    }

    private void Update()
    {
        // 건파우더 관련 로직은 PlayerGunpowderController에서 처리
        // 궁극기 타이머 업데이트
        if (PhotonView.IsMine && _ultimateController != null)
        {
            _ultimateController.UpdateUltimateChanceTimer();
        }
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
        if (_dieSpriteRendererList.Count > 0)
        {
            _playerMaterial.ApplyMaterialById(id, _dieSpriteRendererList);
        }
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
        if (_ultimateController != null)
        {
            _ultimateController.ExecuteUltimate();
        }
    }
    // 건파우더 감소 관련 메서드는 PlayerGunpowderController로 이동

    private void SetSpriteRendererWhite()
    {
        foreach (var renderer in _playerStat.MySpriteREndererList)
        {
            if (renderer == null) { continue; }
            renderer.color = Color.white;
        }
        foreach (var renderer in _dieSpriteRendererList)
        {
            if (renderer == null) { continue; }
            renderer.color = Color.white;
        }
        _isColorRestored = false; // 색상이 변경됨
    }

    // 경고음 관련 메서드는 PlayerGunpowderController로 이동

    /// <summary>
    /// 경고 펄스 효과 재생 (PlayerGunpowderController에서 호출)
    /// </summary>
    public void PlayPreExplosionPulse()
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
                    Color originalColor = kv.Value; // 원본 색상 참조 (읽기 전용)
                    Color.RGBToHSV(originalColor, out float _, out float _, out float v);
                    Color redCol = Color.HSVToRGB(0f, MAX_RED_SATURATION, v);
                    redCol.a = originalColor.a;
                    kv.Key.color = redCol; // 스프라이트 색상만 변경
                }
                _isColorRestored = false; // 색상이 변경됨
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

        // DOTween이 중단될 때도 색상 복원
        seq.OnKill(() =>
        {
            RestoreOriginalColors();
        });

        _preExplosionPulseTween = seq;
    }

    /// <summary>
    /// 경고 펄스 효과 중단 (PlayerGunpowderController에서 호출)
    /// </summary>
    public void StopPreExplosionPulse(bool resetScale)
    {
        if (_preExplosionPulseTween != null)
        {
            _preExplosionPulseTween.Kill(false);
            _preExplosionPulseTween = null;
            // OnKill 콜백에서 이미 RestoreOriginalColors가 호출됨
        }
        else
        {
            // Tween이 없었다면 색상이 이미 복원된 상태거나 복원이 필요한 상태
            // 안전을 위해 한 번 더 복원 (중복 호출이지만 한 번만 실행됨)
            RestoreOriginalColors();
        }

        if (resetScale)
        {
            transform.localScale = _defaultLocalScale;
        }
    }

    /// <summary>
    /// 저장된 원본 색상으로 스프라이트를 복구합니다.
    /// 모든 색상 효과가 끝날 때 반드시 이 메서드를 호출하여 원본 색상으로 돌아갑니다.
    /// </summary>
    public void RestoreOriginalColors()
    {
        // 이미 복원된 상태라면 중복 실행 방지
        if (_isColorRestored)
        {
            return;
        }

        if (_originalColorMap != null && _originalColorMap.Count > 0)
        {
            int restoredCount = 0;
            foreach (var kv in _originalColorMap)
            {
                if (kv.Key != null)
                {
                    // 원본 색상으로 복원
                    kv.Key.color = kv.Value;
                    restoredCount++;
                }
            }

            _isColorRestored = true;
        }
        else
        {
            // 원본 색상 정보가 없는 경우 흰색으로 설정 (비상 조치)
            Debug.LogWarning("[Player] 원본 색상 정보가 없습니다. 흰색으로 복원합니다.");
            SetSpriteRendererWhite();
            _isColorRestored = true;
        }
    }

    public void RPC_PlayExplosionEffect()
    {
        if (!PhotonView.IsMine)
        {
            return;
        }
        PhotonView.RPC(nameof(PlayExplosionEffect), RpcTarget.All);
    }

    [PunRPC]
    private void PlayExplosionEffect()
    {
        if (ExplosionEffectPrefab != null)
        {
            VFXPool.Instance.Play(ExplosionEffectPrefab.name, transform.position);
        }
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
    /// 공격 없음 경고를 완전히 해제 (PlayerGunpowderController로 위임)
    /// </summary>
    public void ClearNoAttackWarning()
    {
        if (_gunpowderController != null)
        {
            _gunpowderController.ClearNoAttackWarning();
        }
    }

    /// <summary>
    /// 공격을 하면 타이머 초기화 (PlayerGunpowderController로 위임)
    /// </summary>
    public void ResetGunPowderDecreaseWithoutAttackTimer()
    {
        if (_gunpowderController != null)
        {
            _gunpowderController.ResetGunPowderDecreaseWithoutAttackTimer();
        }
    }

    /// <summary>
    /// 경고 색상 업데이트 (PlayerGunpowderController에서 호출)
    /// </summary>
    public void UpdateWarningColor(float targetSaturation)
    {
        if (_originalColorMap != null)
        {
            foreach (var kv in _originalColorMap)
            {
                if (kv.Key == null) { continue; }
                Color originalColor = kv.Value; // 원본 색상 참조 (읽기 전용)
                Color.RGBToHSV(originalColor, out float _, out float _, out float v);
                Color newColor = Color.HSVToRGB(0f, targetSaturation, v);
                newColor.a = originalColor.a;
                kv.Key.color = newColor; // 스프라이트 색상만 변경
            }
            _isColorRestored = false; // 색상이 변경됨
        }
    }

    /// <summary>
    /// 펄스 효과가 활성화되어 있는지 확인
    /// </summary>
    public bool IsPreExplosionPulseActive()
    {
        return _preExplosionPulseTween != null && _preExplosionPulseTween.IsActive();
    }

    public void PlayerTeamCheck()
    {

    }

    public void TakeDamage(int damage, int maxDamage, int HealPercent, Vector3 attackerBomb, int attackerViewId, int attackerActorNumber, bool isFallingOut, bool isNormalAttack)
    {
        if (_damageController != null)
        {
            _damageController.TakeDamage(damage, maxDamage, HealPercent, attackerBomb, attackerViewId, attackerActorNumber, isFallingOut, isNormalAttack);
        }
    }

    /// <summary>
    /// 자신의 팀을 CustomProperties에서 동기화
    /// </summary>
    private void SyncTeamFromCustomProperties()
    {
        if (PhotonView.Owner != null && PhotonView.Owner.CustomProperties.ContainsKey(EProperties.Team.ToString()))
        {
            _playerStat.Team = (EInGameTeam)PhotonView.Owner.CustomProperties[EProperties.Team.ToString()];
        }
    }

    /// <summary>
    /// 모든 플레이어의 팀을 CustomProperties에서 동기화
    /// </summary>
    private void SyncAllPlayersTeam()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.ContainsKey(EProperties.Team.ToString()))
            {
                continue;
            }

            EInGameTeam teamFromProperties = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];

            // 해당 플레이어의 GameObject 찾기
            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
            GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject[] allPlayerObjects = new GameObject[playerObjects.Length + enemyObjects.Length];
            playerObjects.CopyTo(allPlayerObjects, 0);
            enemyObjects.CopyTo(allPlayerObjects, playerObjects.Length);

            foreach (var obj in allPlayerObjects)
            {
                PhotonView pv = obj.GetComponent<PhotonView>();
                if (pv != null && pv.Owner != null && pv.Owner.ActorNumber == player.ActorNumber)
                {
                    PlayerStat stat = obj.GetComponent<PlayerStat>();
                    if (stat != null)
                    {
                        stat.Team = teamFromProperties;
                        break;
                    }
                }
            }
        }
    }

    public void RPC_ReleaseGunPowder(Vector3 explosionOrigin, int attackerViewId, int count = 3, float spreadAngle = 30f,
     float distance = 1.0f, bool isFallingOut = true)
    {
        if (_damageController != null)
        {
            _damageController.RPC_ReleaseGunPowder(explosionOrigin, attackerViewId, count, spreadAngle, distance, isFallingOut);
        }
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
        SpriteFlipx();
    }

    private void SpriteFlipx()
    {
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

        foreach (SpriteRenderer spriteRenderer in _dieSpriteRendererList)
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
            && stateName != nameof(PlayerObserveState)
            && stateName != nameof(PlayerLastDieState))
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
                case "PlayerLastDieState":
                    playerFSM.ChangeState<PlayerLastDieState>();
                    break;
                case "PlayerCrabHoldedState":
                    playerFSM.ChangeState<PlayerCrabHoldedState>();
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

    public void InvokeNormalAttack()
    {
        OnNormalAttack?.Invoke();  // 일반 공격 전용 이벤트
    }

    public void InvokeSpecialAttack()
    {
        OnSpecialAttack?.Invoke(); // 특수 공격 전용 이벤트
    }

    /// <summary>
    /// 피격 시 마지막 데미지 비율을 기록하고 피격 이벤트를 발생시킨다.
    /// (PlayerDamageController에서 호출)
    /// </summary>
    public void RegisterHitDamage(int damage, int maxDamage)
    {
        _lastDamageRatio = maxDamage > 0 ? Mathf.Clamp01((float)damage / maxDamage) : 1f;
        OnHit?.Invoke();
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
        // 스킨 슬롯 부모 아래의 모든 스프라이트 렌더러를 끄고, 쿨타임 후 복구
        if (_skinManager is IPlayerSkinManager sm)
        {
            ToggleAllSpriteRenderers(sm.HeadSlotParent, false);
            ToggleAllSpriteRenderers(sm.FaceSlotParent, false);
        }
        _playerStat.MySpriteREndererList[3].enabled = false;

        yield return new WaitForSeconds(BasicBombStat.CoolTime);

        if (_skinManager is IPlayerSkinManager sm2)
        {
            ToggleAllSpriteRenderers(sm2.HeadSlotParent, true);
            ToggleAllSpriteRenderers(sm2.FaceSlotParent, true);
        }
        _playerStat.MySpriteREndererList[3].enabled = true;
    }

    private void ToggleAllSpriteRenderers(Transform root, bool enabled)
    {
        if (root == null) { return; }
        var srs = root.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < srs.Length; i++)
        {
            if (srs[i] != null) { srs[i].enabled = enabled; }
        }
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

    // 데미지 팝업 / 공격자 히트 파티클 관련 로직은 PlayerDamageController로 이동

    // [PunRPC]
    // public void ShowHealthBarForAttacker(int currentHP, int damage, bool isAfterResurrect)
    // {
    //     if (_playerHealthBar != null)
    //     {
    //         _playerHealthBar.ShowHealthBarForAttacker(currentHP, damage, isAfterResurrect);
    //     }
    // }

    /// <summary>
    /// 특수 폭탄 사용 시 건파우더 소모 파티클 생성 (모든 클라이언트에게 표시)
    /// </summary>
    /// <param name="amount">소모한 건파우더 양 (파티클 개수)</param>
    public void RPC_SpawnGunPowderUseParticle()
    {
        if (!PhotonView.IsMine)
        {
            return;
        }
        PhotonView.RPC(nameof(SpawnGunPowderUseParticle), RpcTarget.All);
    }

    [PunRPC]
    private void SpawnGunPowderUseParticle()
    {
        if (_playerGunPowderUsePrefab == null)
        {
            Debug.LogWarning("[Player] _playerGunPowderUsePrefab is null.");
            return;
        }

        if (VFXPool.Instance == null)
        {
            Debug.LogWarning("[Player] VFXPool.Instance is null.");
            return;
        }

        FollowVFX vfx = VFXPool.Instance.Get(_playerGunPowderUsePrefab.name) as FollowVFX;
        if (vfx != null)
        {
            // 플레이어 위쪽으로 호를 그려서 랜덤 위치에 생성
            vfx.PlayAttachedWithArcOffset(transform, arcRadius: 1.5f, arcAngleRange: 90f);
        }
    }

    /// <summary>
    /// 강제로 궁극기 사용가능상태 만들기
    /// 이때는 궁극기 사용가능 시간이 무제한이다.
    /// </summary>
    public void ForceUltimateChance()
    {
        if (_ultimateController != null)
        {
            _ultimateController.ForceUltimateChance();
        }
    }

    public void SetPausedNoAttack()
    {
        if (_gunpowderController != null)
        {
            _gunpowderController.SetPausedNoAttack();
        }
    }

    public void ResetPausedNoAttack()
    {
        if (_gunpowderController != null)
        {
            _gunpowderController.ResetPausedNoAttack();
        }
    }

    /// <summary>
    /// SuperArmor 활성화: 위치 고정 및 속도 0
    /// </summary>
    public void SetSuperArmor()
    {
        if (_rigidbody2D == null) return;

        IsSuperArmor = true;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _rigidbody2D.angularVelocity = 0f;
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
    }

    /// <summary>
    /// SuperArmor 비활성화: 원본 제약 조건 복원
    /// </summary>
    public void ResetSuperArmor()
    {
        if (_rigidbody2D == null) return;

        IsSuperArmor = false;
        _rigidbody2D.constraints = _originalConstraints;
    }

    /// <summary>
    /// 현재 플레이어의 바라보는 방향을 반환
    /// </summary>
    /// <returns>바라보는 방향 (1: 오른쪽, -1: 왼쪽)</returns>
    public int GetFacingDirection()
    {
        return _playerStat.FacingDirection == 1 ? 1 : -1;
    }
}
