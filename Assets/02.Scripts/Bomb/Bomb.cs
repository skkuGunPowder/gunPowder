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

    protected bool _isDestroying;

    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _isDestroying = false;

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
        _isDestroying = false;
        _fuzeTimer = 0f;
        _currentSpeed = 0f;
        _fireDirection = Vector3.zero;
    }

    protected virtual void Update()
    {
        if(_isDestroying)
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
            _fuzeTimer = 0f;
            if(photonView.IsMine)
            {
                photonView.RPC(nameof(Explode), RpcTarget.All);
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
            Debug.LogWarning($"ID{ownerViewId}를 찾을 수 없습니다.");
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
                return false;
            }
            
            if (_stat.Priority - otherPriority < 2)
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
        if (_isDestroying)
        {
            return;
        }
        _isDestroying = true;
        transform.DOKill();
        StopAllCoroutines();

        Explosion explosion = ExplosionPool.Instance.Get(ExplosionPrefab.name);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;
        explosion.Explode(_stat.IsFallingOut, _ownerPhotonview);

        if (_vfx != null)
        {
            _vfx.transform.SetParent(transform);
        }
        
        DestroyCollector.Instance.PhotonLazyDestory(gameObject, photonView);
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
        if (_vfx != null)
        {
            _vfx.transform.SetParent(null);
            _vfx.gameObject.SetActive(false);
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