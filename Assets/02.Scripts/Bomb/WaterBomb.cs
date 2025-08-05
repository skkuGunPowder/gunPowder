using Photon.Pun;
using UnityEngine;
using DG.Tweening;

public class WaterBomb : Bomb
{
    public const string ID = "BO0005";

    public float _wobbleAmount = 0.2f;     // 출렁이는 크기 변화 비율
    public float _wobbleDuration = 0.3f;   // 출렁이는 애니메이션 시간
    private Vector3 _originalScale;
    private Tween _wobbleTween;


    protected override void Init()
    {
        base.Init();
        SetStat(ID);
        _originalScale = transform.localScale;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_wobbleTween != null && _wobbleTween.IsActive())
        {
            _wobbleTween.Kill();
        }

        ContactPoint2D contact = other.contacts[0];
        // Vector2 normal = contact.normal.normalized;
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
            transform.DOScale(_originalScale, _wobbleDuration / 2f)
                .SetEase(Ease.InQuad);
        });


        if (CheckPriority(other))
        {
            return;
        }
    }

    private void OnDestroy()
    {
        if (_wobbleTween != null && _wobbleTween.IsActive())
        {
            _wobbleTween.Kill();
        }
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
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void BoostBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void SmashBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }
}
