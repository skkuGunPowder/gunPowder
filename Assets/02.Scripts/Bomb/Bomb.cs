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


    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
        _rigidBody = GetComponent<Rigidbody2D>();

        Init();

        if (TrailVFXPrefab != null)
        {
            _vfx = Instantiate(TrailVFXPrefab);
            _vfx.gameObject.SetActive(false);
        }
    }

    protected virtual void OnEnable()
    {
        if(!PhotonView.IsMine)
        {
            return;
        }

        _fuzeTimer = 0f;
        _currentSpeed = 0f;
        _fireDirection = Vector3.zero;
    }

    protected virtual void Update()
    {
        if(!PhotonView.IsMine)
        {
            return;
        }

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
            PhotonView.RPC(nameof(Explode), RpcTarget.All);
        }
    }
    
    protected virtual void Init()
    {

    }

    [PunRPC]
    public void SetOwner(int ownerViewId)
    {
        _ownerTransform = PhotonView.Find(ownerViewId).transform;
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
                PhotonView.RPC(nameof(Explode), RpcTarget.All);
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
        // 폭발 프리펩 인스턴싱
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

        if (PhotonNetwork.IsMasterClient)
        {
            // 플레이어 포톤뷰로 폭탄 회수
            ObjectPoolManager.Instance.ReleaseObject(gameObject.name, gameObject);
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