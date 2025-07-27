using Photon.Pun;
using UnityEngine;

public class Bomb : MonoBehaviourPun, IBomb
{
    public Explosion ExplosionPrefab;
    protected Rigidbody2D _rigidBody;
    protected BombStat _stat;
    protected Vector3 _fireDirection;
    protected float _currentSpeed;
    private float _fuzeTimer;

    protected Transform _ownerTransform;

    public Transform TrailVFXPosition;
    public ParticleSystem TrailVFXPrefab;
    protected ParticleSystem _vfx;

    public PhotonView PhotonView;

    // 폭발 상태를 추적하여 중복 폭발 방지
    private bool _hasExploded = false;
    private bool _hasRequestedDestroy = false; // 파괴 요청 중복 방지

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
        _hasExploded = false;
        _hasRequestedDestroy = false;
    }

    protected virtual void Update()
    {
        // 타이머는 소유자만 관리
        if(!PhotonView.IsMine)
        {
            return;
        }

        // VFX 위치 업데이트는 모든 클라이언트에서
        if (_vfx != null)
        {
            _vfx.transform.position = TrailVFXPosition.position;
        }

        if (_stat == null)
        {
            return;
        }

        _fuzeTimer += Time.deltaTime;
        if (_fuzeTimer >= _stat.FuzeTime && !_hasExploded)
        {
            _hasExploded = true;
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
            _ownerTransform = ownerPhotonView.transform;
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
        if (other.gameObject.TryGetComponent(out Bomb otherBomb))
        {
            int otherPriority = otherBomb._stat.Priority;
            if (_stat.Priority <= otherPriority)
            {
                if (!_hasExploded)
                {
                    _hasExploded = true;
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
        // 중복 폭발 방지
        if (_hasExploded)
        {
            Debug.Log($"[Bomb] Explode called but already exploded: {gameObject.name}");
            return;
        }
        _hasExploded = true;

        // 폭발 프리펩 인스턴싱 (로컬에서만)
        if (ExplosionPrefab == null)
        {
            Debug.Log("펑(억장 터지는 소리: ExposionPrefab == null)");
        }
        else
        {
            Explosion explosion = Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
            explosion.Explode(_stat.IsFallingOut, _ownerTransform);
        }

        if (_vfx != null)
        {
            _vfx.transform.SetParent(transform);
        }

        // 소유자만 파괴 요청
        if (PhotonView.IsMine && !_hasRequestedDestroy)
        {
            Debug.Log($"[Bomb] Requesting destroy for: {gameObject.name} (ViewID: {PhotonView.ViewID})");
            _hasRequestedDestroy = true;
            
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

        // TODO
        // Pool 만들면 회수 코드 작성
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