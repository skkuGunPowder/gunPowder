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
    private bool isDestroyed = false; // 중복 파괴 방지 플래그
    private const float SLOW = 5f;
    private const float NORMAL = 10f;
    private Coroutine _selfDestructionCoroutine;

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
        isDestroyed = false; // 재사용 시 초기화
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
            photonView.RPC(nameof(Explode), RpcTarget.All);
            PhotonNetwork.Destroy(gameObject);
        }

        if (_bombVelocity == EBombVelocity.NORMAL && !_isFuzeActivate)
        {
            if (other.gameObject.TryGetComponent(out IDamagable damagableObject))
            {
                photonView.RPC(nameof(Explode), RpcTarget.All);
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                _selfDestructionCoroutine = StartCoroutine(ActivateFuzeCoroutine(0.7f));
            }
        }
    }

    private IEnumerator ActivateFuzeCoroutine(float fuzeTime)
    {
        _isFuzeActivate = true;
        yield return new WaitForSeconds(fuzeTime);
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(Explode), RpcTarget.All);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = 0f;
        isDestroyed = false; // 재사용 시 초기화
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
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(Explode), RpcTarget.All);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    public override void SmashBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = _stat.Speed;
        _rigidBody.AddForce(_fireDirection * _currentSpeed, ForceMode2D.Impulse);
    }

    protected override void OnDestroy()
    {
        if (_selfDestructionCoroutine != null)
        {
            StopCoroutine(_selfDestructionCoroutine);
        }
        
        base.OnDestroy();
    }
}