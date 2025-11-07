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
        SoundManager.Instance.PlayLocalSound(this.GetType().Name, transform, 0, true);
          Vector3 originalScale = transform.localScale;

        // 기존 트윈이 있다면 정리
        if (_pulseTween != null && _pulseTween.IsActive())
        {
            _pulseTween.Kill();
        }

        // DOTween으로 무한 반복 펄스 트윈 생성
        _pulseTween = transform.DOScale(originalScale * pulseScale, pulseDuration / 2f)
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(Ease.InOutSine) // 부드러운 감속/가속
        .SetAutoKill(false) // 수동으로 Kill하도록 설정
        .SetTarget(transform); // Transform과 함께 자동 정리
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
        
        if (_isExploding) // 이미 폭발 중인 경우 중복 호출 방지
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
        if (_isExploding) return; // 중복 파괴 방지
        _isExploding = true;

        // 트윈 정리
        CleanupTweens();

        //base.Explode();
        // 폭발 프리펩 인스턴싱 (로컬에서만)
        Explosion explosion = ExplosionPool.Instance.Get(ExplosionPrefab.name);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;
        explosion.Explode(_stat.IsFallingOut, _ownerPhotonview, true);   

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

    private void OnDisable()
    {
        CleanupTweens();
    }

    private void OnDestroy()
    {
        CleanupTweens();
    }

    private void CleanupTweens()
    {
        if (_pulseTween != null && _pulseTween.IsActive())
        {
            _pulseTween.Kill();
            _pulseTween = null;
        }
    }
}