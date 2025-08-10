using Photon.Pun;
using UnityEngine;

public class GunPowder : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private BoxCollider2D _collider;
    private float _timer = 0f;
    private float _colliderOnTime = 0.2f;
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


    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        _collider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider.enabled = false;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if(_timer > _colliderOnTime)
        {
            _collider.enabled = true;
        }
    }

    /// <summary>
    /// 건파우더 생성 어태커 퓨 아이디, 낙출 여부, 랜덤덤
    /// </summary>
    /// <param name="info"></param>
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
                if (attackerView != null)
                {
                    _target = attackerView.transform;
                }
            }

            // 스프라이트 설정 - 자신이 생성한 건파우더인지 확인
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
    /// 건파우더 생성자에 따라 스프라이트 설정
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
            }
            else
            {
                _spriteRenderer.sprite = _enemySprite;
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
    public void SetTarget(int targetViewId)
    {
        PhotonView targetView = PhotonView.Find(targetViewId);
        if (targetView != null)
            _target = targetView.transform;
    }
}
