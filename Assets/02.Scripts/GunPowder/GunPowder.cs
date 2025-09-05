using Photon.Pun;
using UnityEngine;
using SpriteTrail;

/// <summary>
/// 건파우더(투사체) 네트워크 오브젝트
/// 
/// 역할:
/// - Photon 인스턴스 데이터 수신 및 초기 설정(타겟/낙출 여부/랜덤 시드/발사자)
/// - 내/적 소유에 따른 스프라이트/트레일 프리셋 적용
/// - 낙출 모드(GunPowderRelease) 또는 베지어 추적 모드(GunPowderBezierCurve) 활성화
/// - 생성 직후 짧은 지연 후 충돌 활성화, 런타임 타겟 RPC 갱신 지원
/// 
/// 동작 방식:
/// 1. Awake에서 필수 컴포넌트 캐싱, Collider 비활성화
/// 2. Update에서 일정 시간 후 Collider 활성화
/// 3. OnPhotonInstantiate에서 인스턴스 데이터 파싱 → 모드/타겟/소유 구분 → 컴포넌트 활성화 전환
/// 4. SetTarget RPC로 타겟 동기화
/// </summary>
public class GunPowder : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private BoxCollider2D _collider;
    private float _timer = 0f;
    private float _colliderOnTime = 0.2f;
    [SerializeField]
    private float _lifetimeSeconds = 10f;
    private float _lifeTimer = 0f;
    private bool _hasRequestedLifeDestroy = false;

    [Header("Lifetime Flicker")]
    [SerializeField]
    private float _flickerStartSeconds = 8f;
    [SerializeField]
    private float _flickerIntervalSeconds = 0.05f;
    private bool _isFlickering = false;
    private float _nextFlickerTime = 0f;
    private bool _currentVisible = true;

    private Transform _target;
    public Transform Target => _target;
    private bool _isFallingOut;
    public bool IsFallingOut => _isFallingOut;
    private int _randomSeed; // 랜덤 시드
    private int _sourceViewId; // 건파우더 출발 주체 ViewID
    public int SourceViewId => _sourceViewId;

    private PhotonView _photonView;
    public PhotonView PhotonView => _photonView;

    private SpriteRenderer _spriteRenderer;
    public Sprite _mySprite;
    public Sprite _enemySprite;
    
    private SpriteTrail.SpriteTrail _spriteTrail;
    public TrailPreset MyTrailPreset;
    public TrailPreset EnemyTrailPreset;


    /// <summary>
    /// 필수 컴포넌트 캐싱 및 초기 비활성화 설정
    /// </summary>
    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        _collider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteTrail = GetComponent<SpriteTrail.SpriteTrail>();
        _collider.enabled = false;
    }

    /// <summary>
    /// 생성 후 일정 시간 경과 시 Collider 활성화
    /// </summary>
    private void Update()
    {
        _timer += Time.deltaTime;
        if(_timer > _colliderOnTime)
        {
            _collider.enabled = true;
        }

        // 일정 시간 경과 후 삭제(네트워크 동기화)
        _lifeTimer += Time.deltaTime;

        // 8초 이후 깜박임 시작
        if (!_isFlickering && _lifeTimer >= _flickerStartSeconds)
        {
            _isFlickering = true;
            _nextFlickerTime = 0f;
            _currentVisible = true;
        }
        if (_isFlickering && _lifeTimer >= _nextFlickerTime)
        {
            _currentVisible = !_currentVisible;
            if (_spriteRenderer != null) _spriteRenderer.enabled = _currentVisible;
            if (_spriteTrail != null) _spriteTrail.enabled = _currentVisible;
            _nextFlickerTime = _lifeTimer + _flickerIntervalSeconds;
        }

        if (!_hasRequestedLifeDestroy && _lifeTimer >= _lifetimeSeconds)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                InstantiateDestroyManager.Instance.RequestDestroy(_photonView.ViewID);
            }
            _hasRequestedLifeDestroy = true;
        }
    }

    /// <summary>
    /// Photon 인스턴스 데이터 파싱(공격자/낙출 여부/랜덤 시드/발사자) 및 모드 전환
    /// </summary>
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instData = photonView.InstantiationData;
        if (instData != null && instData.Length >= 3) // 3개로 변경
        {
            int attackerViewId = (int)instData[0];
            _isFallingOut = (bool)instData[1];
            _randomSeed = (int)instData[2]; // 랜덤 시드 추가
            if (instData.Length >= 4)
            {
                _sourceViewId = (int)instData[3]; // 건파우더 출발 주체 ViewID 추가
            }

            if (attackerViewId != 0)
            {
                PhotonView attackerView = PhotonView.Find(attackerViewId);
                if (attackerView != null && attackerView.gameObject != null && attackerView.gameObject.activeInHierarchy)
                {
                    _target = attackerView.transform;
                }
            }

            // 스프라이트/트레일 설정 - 자신이 생성한 건파우더인지 확인
            SetSpriteBasedOnOwner();

            // 컴포넌트 활성화/비활성화 처리
            var release = GetComponent<GunPowderRelease>();
            release.SetRandomSeed(_randomSeed);
            var bezier = GetComponent<GunPowderBezierCurve>();
            if (_isFallingOut)
            {
                if (release != null) release.enabled = true;
                if (bezier != null) bezier.enabled = false;
            }
            else
            {
                if (release != null) release.enabled = false;
                if (bezier != null) bezier.enabled = true;
            }
        }
        else
        {
            Debug.LogError($"[GunPowder] instData가 null이거나 길이가 부족합니다. Length: {instData?.Length}");
        }
        
        
    }

    /// <summary>
    /// 건파우더 생성자에 따라 스프라이트/트레일 프리셋 설정
    /// </summary>
    private void SetSpriteBasedOnOwner()
    {
        if (_spriteRenderer == null) return;

        // 현재 로컬 플레이어의 PhotonView 찾기
        Player localPlayer = FindLocalPlayer();
        if (localPlayer != null)
        {
            // 자신이 생성한 건파우더인지 확인
            if (_sourceViewId == localPlayer.PhotonView.ViewID)
            {
                _spriteRenderer.sprite = _mySprite;
                _spriteTrail.SetTrailPreset(MyTrailPreset);
            }
            else
            {
                _spriteRenderer.sprite = _enemySprite;
                _spriteTrail.SetTrailPreset(EnemyTrailPreset);
            }
        }
        else
        {
            // 로컬 플레이어를 찾을 수 없는 경우 기본적으로 적 스프라이트 사용
            _spriteRenderer.sprite = _enemySprite;
        }
    }

    /// <summary>
    /// 현재 로컬 플레이어 찾기
    /// </summary>
    private Player FindLocalPlayer()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (Player player in players)
        {
            if (player.PhotonView.IsMine)
            {
                return player;
            }
        }
        return null;
    }

    [PunRPC]
    /// <summary>
    /// 네트워크 RPC로 타겟 ViewID를 받아 타겟 Transform 설정
    /// </summary>
    public void SetTarget(int targetViewId)
    {
        PhotonView targetView = PhotonView.Find(targetViewId);
        if (targetView != null && targetView.gameObject != null && targetView.gameObject.activeInHierarchy)
            _target = targetView.transform;
    }
}
