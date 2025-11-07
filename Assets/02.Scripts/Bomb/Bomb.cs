using Photon.Pun;
using UnityEngine;

public class Bomb : MonoBehaviourPun, IBomb
{
    public Explosion ExplosionPrefab;
    protected Rigidbody2D _rigidBody;
    protected BombStat _stat;
    protected Vector3 _fireDirection;
    protected float _currentSpeed;
    protected float _fuzeTimer;
    protected PhotonView _ownerPhotonview;

    public Transform TrailVFXPosition;
    public ParticleSystem TrailVFXPrefab;
    protected ParticleSystem _vfx;

    public PhotonView PhotonView;

    // 중복 파괴 방지 플래그
    protected bool isDestroyed = false;

    // 네트워크 동기화를 위한 생성 시간 기록
    protected double _spawnTime;

    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
        _rigidBody = GetComponent<Rigidbody2D>();

        Init();

        // VFX는 모든 클라이언트에서 개별적으로 생성
        if (TrailVFXPrefab != null)
        {
            _vfx = Instantiate(TrailVFXPrefab);
            _vfx.gameObject.SetActive(false);
        }
    }

    protected virtual void OnEnable()
    {
        // 모든 클라이언트에서 초기화
        _fuzeTimer = 0f;
        _currentSpeed = 0f;
        _fireDirection = Vector3.zero;
        isDestroyed = false;
        _spawnTime = PhotonNetwork.Time;
    }

    protected virtual void Update()
    {
        // VFX 위치 업데이트는 모든 클라이언트에서
        if (_vfx != null)
        {
            _vfx.transform.position = TrailVFXPosition.position;
        }

        // 타이머는 소유자만 관리
        if(!PhotonView.IsMine)
        {
            return;
        }

        if (_stat == null)
        {
            return;
        }

        // PhotonNetwork.Time 사용으로 동기화 개선
        double currentTime = PhotonNetwork.Time;
        double elapsedTime = currentTime - _spawnTime;

        if (elapsedTime >= _stat.FuzeTime && !isDestroyed)
        {
            PhotonView.RPC(nameof(Explode), RpcTarget.All);
        }
    }
    
    protected virtual void Init()
    {

    }

    [PunRPC]
    public void SetOwner(int ownerViewId)
    {
        PhotonView ownerPhotonView = PhotonView.Find(ownerViewId);
        if (ownerPhotonView != null)
        {
            _ownerPhotonview = ownerPhotonView;
        }
        else
        {
            Debug.LogWarning($"Owner PhotonView with ID {ownerViewId} not found");
        }
    }

    protected void SetStat(string id)
    {
        _stat = ItemDatabase.Instance.GetStat<BombStat>(id);
    }

    protected bool CheckPriority(Collision2D other)
    {
        if (isDestroyed)
        {
            return true; // 이미 파괴 중이면 더 이상 처리하지 않음
        }

        if (other.gameObject.TryGetComponent(out Bomb otherBomb))
        {
            int otherPriority = otherBomb._stat.Priority;
            if (_stat.Priority <= otherPriority)
            {
                if (!isDestroyed)
                {
                    PhotonView.RPC(nameof(Explode), RpcTarget.All);
                }
            }
            else if (_stat.Priority - otherPriority < 2)
            {
                _rigidBody.linearVelocity /= 2;
                return true;
            }
        }
        return false;
    }

    [PunRPC]
    public virtual void Explode()
    {
        // 중복 파괴 방지
        if (isDestroyed)
        {
            return;
        }
        isDestroyed = true;

        // 폭발 프리펩 인스턴싱 (로컬에서만)
        Explosion explosion = ExplosionPool.Instance.Get(ExplosionPrefab.name);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;

        // Owner null 체크 추가
        if (_ownerPhotonview != null)
        {
            explosion.Explode(_stat.IsFallingOut, _ownerPhotonview);
        }
        else
        {
            Debug.LogWarning($"[Bomb] Owner PhotonView is null, explosion without owner: {gameObject.name}");
            explosion.Explode(_stat.IsFallingOut, null);
        }

        if (_vfx != null)
        {
            _vfx.transform.SetParent(transform);
        }

        // 소유자만 파괴 요청
        if (PhotonView.IsMine)
        {
            // 추가 안전장치: PhotonView가 여전히 유효한지 확인
            if (PhotonView != null && PhotonView.ViewID != 0)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"[Bomb] PhotonView is invalid, destroying locally: {gameObject.name}");
                Destroy(gameObject);
            }
        }
    }

    public BombStat GetBombStat()
    {
        return _stat;
    }

    public virtual void ResetFuze()
    {
        _fuzeTimer = 0f;
    }

    public virtual void PauseBomb()
    {

    }

    public virtual void ResumeBomb()
    {

    }

    [PunRPC]
    // 폭탄 두기
    public virtual void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
    }

    [PunRPC]
    // 폭탄 던지기 (곡사)
    public virtual void ThrowBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
    }

    [PunRPC]
    // 폭탄 직선으로 던지기
    public virtual void ThrowBombStraight(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
    }

    [PunRPC]
    // 폭탄 부스트
    public virtual void BoostBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
    }

    [PunRPC]
    // 폭탄 내려 찍기
    public virtual void SmashBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
    }
}