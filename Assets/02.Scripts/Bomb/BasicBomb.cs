using System.Collections;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

public enum EBombVelocity
{
    SLOW,
    NORMAL,
    FAST
}

public class BasicBomb : Bomb
{
    public const string ID = "BO0001";
    private EBombVelocity _bombVelocity = EBombVelocity.SLOW;
    private bool _isFuzeActivate;
    // isDestroyed는 베이스 클래스에서 관리
    private const float SLOW = 5f;
    private const float NORMAL = 10f;

    private Tween _pulseTween;
    [SerializeField] private float pulseScale = 2f; // 펄스 크기
    [SerializeField] private float pulseDuration = 0.1f; // 펄스 지속 시간

    protected override void OnEnable()
    {
        base.OnEnable();
        Init();
    }

    protected override void Init()
    {
        base.Init();
        SetStat(ID);
        _isFuzeActivate = false;
        // isDestroyed는 base.OnEnable()에서 초기화됨
        SoundManager.Instance.PlayLocalSound(this.GetType().Name, transform, 0, true);
        Vector3 originalScale = transform.localScale;

        // DOTween으로 무한 반복 펄스 트윈 생성
        _pulseTween = transform.DOScale(originalScale * pulseScale, pulseDuration / 2f)
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(Ease.InOutSine); // 부드러운 감속/가속
    }

    protected override void Update()
    {
        base.Update();
        if (_rigidBody.linearVelocity.magnitude >= NORMAL)
        {
            _bombVelocity = EBombVelocity.FAST;
        }
        if (_rigidBody.linearVelocity.magnitude < NORMAL)
        {
            _bombVelocity = EBombVelocity.NORMAL;
        }
        if (_rigidBody.linearVelocity.magnitude < SLOW)
        {
            _bombVelocity = EBombVelocity.SLOW;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(!PhotonView.IsMine)
        {
            return;
        }

        if (other.gameObject.tag == "Player" || other.gameObject.tag == "Immune")
        {
            return;
        }

        if (CheckPriority(other))
        {
            return;
        }
        
        if (isDestroyed) // 이미 파괴된 경우 중복 호출 방지
        {
            return;
        }

        if (_bombVelocity == EBombVelocity.FAST)
        {
            PhotonView.RPC(nameof(Explode), RpcTarget.All);
        }

        if (_bombVelocity == EBombVelocity.NORMAL && !_isFuzeActivate)
        {
            if (other.gameObject.TryGetComponent(out IDamagable damagableObject))
            {
                PhotonView.RPC(nameof(Explode), RpcTarget.All);
            }
            else
            {
                StartCoroutine(ActivateFuzeCoroutine(0.7f));
            }
        }
    }

    private IEnumerator ActivateFuzeCoroutine(float fuzeTime)
    {
        _isFuzeActivate = true;
        yield return new WaitForSeconds(fuzeTime);
        PhotonView.RPC(nameof(Explode), RpcTarget.All);
    }

    [PunRPC]
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = 0f;
        // isDestroyed는 OnEnable()에서 초기화됨
    }

    [PunRPC]
    public override void ThrowBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = _stat.Speed;
        _rigidBody.AddForce(_fireDirection * _currentSpeed, ForceMode2D.Impulse);
    }

    [PunRPC]
    public override void ThrowBombStraight(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = _stat.Speed * 2f;
        _rigidBody.AddForce(_fireDirection * _currentSpeed, ForceMode2D.Impulse);
    }

    [PunRPC]
    public override void BoostBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        PhotonView.RPC(nameof(Explode), RpcTarget.All);
    }

    [PunRPC]
    public override void SmashBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = _stat.Speed;
        _rigidBody.AddForce(_fireDirection * _currentSpeed, ForceMode2D.Impulse);
    }

    [PunRPC]
    public override void Explode()
    {
        // 중복 파괴 방지는 base.Explode()에서 처리
        if (isDestroyed)
        {
            return;
        }

        if (_pulseTween != null && _pulseTween.IsActive())
        {
            _pulseTween.Kill();
        }

        // BasicBomb 전용 폭발 로직 (isNormalAttack=true 파라미터)
        isDestroyed = true;

        Explosion explosion = ExplosionPool.Instance.Get(ExplosionPrefab.name);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;

        // Owner null 체크
        if (_ownerPhotonview != null)
        {
            explosion.Explode(_stat.IsFallingOut, _ownerPhotonview, true);
        }
        else
        {
            Debug.LogWarning($"[BasicBomb] Owner PhotonView is null");
            explosion.Explode(_stat.IsFallingOut, null, true);
        }

        if (_vfx != null)
        {
            _vfx.transform.SetParent(transform);
        }

        // 소유자만 파괴 요청
        if (PhotonView.IsMine)
        {
            if (PhotonView != null && PhotonView.ViewID != 0)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"[BasicBomb] PhotonView is invalid, destroying locally: {gameObject.name}");
                Destroy(gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        // DOTween 정리
        if (_pulseTween != null && _pulseTween.IsActive())
        {
            _pulseTween.Kill();
        }
    }
}