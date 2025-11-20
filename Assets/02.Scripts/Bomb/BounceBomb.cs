using Photon.Pun;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;

public class BounceBomb : Bomb
{
    public const string ID = "BO0011";

    private Vector3 _originalScale;
    private Tween _wobbleTween;
    private float _wobbleAmount = 0.6f;
    private float _wobbleDuration = 0.1f;

    private float _delayTime = 0.1f;
    private float _bounceSpeedY = 24f;

    [Header("수치 조정")]
    [SerializeField] private float _bounceDuration = 0.3f;
    [SerializeField] private Transform _bottomTopChecker;
    [SerializeField] private LayerMask _groundLayer;

    private bool _isGrounded = false;
    private bool _isCharging = false;



    protected override void Init()
    {
        base.Init();
        SetStat(ID);
        _originalScale = transform.localScale;
    }

    private void FixedUpdate()
    {
        if(_isCharging)
        {
            return;
        }


        _bottomTopChecker.localRotation = Quaternion.identity;
        RaycastHit2D bottomHit = Physics2D.Raycast(_bottomTopChecker.position, Vector2.down, 0.5f, _groundLayer);
        RaycastHit2D topHit = Physics2D.Raycast(_bottomTopChecker.position, Vector2.up, 0.5f, _groundLayer);
        if (bottomHit.collider != null)
        {
            if (!_isGrounded && !_isCharging)
            {
                StartCoroutine(DelayCoroutine(_delayTime, true));
            }
            return;
        }
        else if (topHit.collider != null)
        {
            if (!_isGrounded && !_isCharging)
            {
                StartCoroutine(DelayCoroutine(_delayTime, false));
            }
            return;
        }
        else
        {
            _isGrounded = false;
        }

        _rigidBody.linearVelocity = new Vector2(_fireDirection.x * _currentSpeed, _rigidBody.linearVelocityY);
    }

    private IEnumerator DelayCoroutine(float time, bool isBottom)
    {
        _isCharging = true;
        _rigidBody.linearVelocity = Vector2.zero;
        _rigidBody.freezeRotation = true;

        yield return new WaitForSeconds(time);

        _rigidBody.freezeRotation = false;
        if (isBottom)
        {
            _rigidBody.linearVelocity = new Vector2(_fireDirection.x * _currentSpeed, _bounceSpeedY);
        }
        else
        {
            _rigidBody.linearVelocity = new Vector2(_fireDirection.x * _currentSpeed, -_bounceSpeedY);
        }
        _isGrounded = true;
        _isCharging = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(_isDestroying)
        {
            return;
        }

        ContactPoint2D contact = collision.contacts[0];
        Vector2 normal = transform.InverseTransformDirection(contact.normal.normalized);

        // 축 계산: 충돌 방향에 수직한 축으로 부풀리기
        float xSquash = 1f - Mathf.Abs(normal.x) * _wobbleAmount;
        float ySquash = 1f - Mathf.Abs(normal.y) * _wobbleAmount;

        // 반대 방향으로는 살짝 팽창 (보존하는 느낌)
        float xStretch = 1f + Mathf.Abs(normal.y) * _wobbleAmount * 0.5f;
        float yStretch = 1f + Mathf.Abs(normal.x) * _wobbleAmount * 0.5f;


        Vector3 squashedScale = new Vector3(
            _originalScale.x * xSquash * xStretch,
            _originalScale.y * ySquash * yStretch,
            _originalScale.z
        );

        _wobbleTween = transform.DOScale(squashedScale, _wobbleDuration / 2f)
        .SetEase(Ease.OutQuad)
        .SetAutoKill(false)
        .SetTarget(transform)
        .OnComplete(() =>
        {
            // Transform 파괴 여부 확인
            if (transform != null && gameObject != null)
            {
                transform.DOScale(_originalScale, _wobbleDuration / 2f)
                    .SetEase(Ease.InQuad)
                    .SetTarget(transform);
                SoundManager.Instance.PlayLocalSound("BounceBomb_1", transform);
            }
        });

        if (collision.gameObject == _ownerPhotonview.gameObject)
        {
            return;
        }

        if (collision.gameObject.TryGetComponent(out IDamagable damagable))
        {
            if (photonView.IsMine)
            {
                photonView.RPC(nameof(Explode), RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public override void ThrowBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = _stat.Speed;
        _rigidBody.linearVelocity = new Vector2(_fireDirection.x * _currentSpeed, _bounceSpeedY);
    }

    [PunRPC]
    public override void BoostBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void SmashBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void ThrowBombStraight(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }
}
