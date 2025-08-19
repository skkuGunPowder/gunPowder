using Photon.Pun;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class BounceBomb : Bomb
{
    public const string ID = "BO0011";

    private Vector3 _originalScale;
    private Tween _wobbleTween;
    private float _wobbleAmount = 0.6f;     // 출렁이는 크기 변화 비율
    private float _wobbleDuration = 0.1f;   // 출렁이는 애니메이션 시간

    private float _delayTime = 0.1f;
    private float _bounceSpeedY = 24f;

    [Header("수치 조정")]
    [SerializeField] private float _bounceDuration = 0.3f;
    [SerializeField] private Transform _bottomChecker;
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
        _bottomChecker.localRotation = Quaternion.identity;
        RaycastHit2D hit = Physics2D.Raycast(_bottomChecker.position, Vector2.down, 0.5f, _groundLayer);
        if (hit.collider != null)
        {
            if (!_isGrounded && !_isCharging)
            {
                Debug.Log($"AddForece!!");
                StartCoroutine(DelayCoroutine(_delayTime));
            }
            return;
        }
        else
        {
            _isGrounded = false;
        }

        _rigidBody.linearVelocity = new Vector2(_fireDirection.x * _currentSpeed, _rigidBody.linearVelocityY);
    }

    private IEnumerator DelayCoroutine(float time)
    {
        _isCharging = true;
        _rigidBody.linearVelocity = Vector2.zero;
        _rigidBody.freezeRotation = true;

        yield return new WaitForSeconds(time);

        _rigidBody.freezeRotation = false;
        _rigidBody.linearVelocity = new Vector2(_fireDirection.x * _currentSpeed, _bounceSpeedY);
        _isGrounded = true;
        _isCharging = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_wobbleTween != null && _wobbleTween.IsActive())
        {
            _wobbleTween.Kill();
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
        .OnComplete(() =>
        {
            transform.DOScale(_originalScale, _wobbleDuration / 2f).SetEase(Ease.InQuad);
            SoundManager.Instance.PlayLocalSound("BounceBomb_1", transform);
        });

        if (collision.gameObject == _ownerPhotonview.gameObject)
        {
            return;
        }

        if (collision.gameObject.TryGetComponent(out IDamagable damagable))
        {
            PhotonView.RPC(nameof(Explode), RpcTarget.All);
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
