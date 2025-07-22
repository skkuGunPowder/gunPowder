using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class Bomb : MonoBehaviour, IBomb
{
    public Explosion ExplosionPrefab;

    protected Rigidbody2D _rigidBody;
    protected BombStat _stat;
    protected Transform _fireTransform;
    protected Vector3 _fireDirection;
    protected float _currentSpeed;

    private float _fuzeTimer;

    private void Awake()
    {
        Init();
    }

    protected virtual void Update()
    {
        if(_stat == null)
        {
            return;
        }
        
        _fuzeTimer += Time.deltaTime;
        if (_fuzeTimer >= _stat.FuzeTime)
        {
            Explode();
        }
    }

    protected virtual void Init()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    protected void SetStat(string id)
    {
        _stat = ItemDatabase.Instance.GetStat<BombStat>(id);
    }

    protected virtual void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent(out Bomb otherBomb))
        {
            int otherPriority = otherBomb._stat.Priority;
            if (_stat.Priority <= otherPriority)
            {
                Explode();
            }
            else if (_stat.Priority - otherPriority < 2)
            {
                _currentSpeed /= 2;
                return;
            }
        }
    }

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
            explosion.Explode(_stat.IsFallingOut);
        }

        Destroy(gameObject);
    }


    [PunRPC]
    // 폭탄 두기
    public virtual void PlaceBomb(Transform fireTransform)
    {
        
    }

    [PunRPC]
    // 폭탄 던지기 (곡사)
    public virtual void ThrowBomb(Transform fireTransform)
    {
        
    }

    [PunRPC]
    // 폭탄 직선으로 던지기
    public virtual void ThrowBombStraight(Transform fireTransform)
    {
        
    }

    [PunRPC]
    // 폭탄 부스트
    public virtual void BoostBomb(Transform fireTransform)
    {
       
    }

    [PunRPC]
    // 폭탄 내려 찍기
    public virtual void SmashBomb(Transform fireTransform)
    {
       
    }
}
