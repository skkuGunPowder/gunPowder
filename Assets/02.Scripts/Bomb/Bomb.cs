using DG.Tweening;
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
        if (_fuzeTimer >= _stat.FuzeTime)
        {
            if (photonView.IsMine)
            {
                photonView.RPC(nameof(Explode), RpcTarget.All);
                PhotonNetwork.Destroy(gameObject);
            }
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
        if (other.gameObject.TryGetComponent(out Bomb otherBomb))
        {
            int otherPriority = otherBomb._stat.Priority;
            if (_stat.Priority <= otherPriority)
            {
                if (photonView.IsMine)
                {
                    photonView.RPC(nameof(Explode), RpcTarget.All);
                    PhotonNetwork.Destroy(gameObject);
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
        Explosion explosion = ExplosionPool.Instance.Get(ExplosionPrefab.name);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;
        explosion.Explode(_stat.IsFallingOut, _ownerPhotonview);

        if (_vfx != null)
        {
            _vfx.transform.SetParent(transform);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
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

    protected virtual void OnDestroy()
    {
        if (transform != null)
        {
            transform.DOKill();
        }
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