using System.Collections;
using UnityEngine;

public enum EBombVelocity
{
    SLOW,
    NORMAL,
    FAST
}

public class BasicBomb : Bomb
{
    public const string ID = "B0001";

    private EBombVelocity _bombVelocity = EBombVelocity.SLOW;
    private bool _isFuzeActivate;

    private const float SLOW = 5f;
    private const float NORMAL = 10f;


    protected override void Init()
    {
        base.Init();

        SetStat(ID);
        _isFuzeActivate = false;
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

    protected override void OnCollisionEnter2D(Collision2D other)
    {
        base.OnCollisionEnter2D(other);

        if (_bombVelocity == EBombVelocity.FAST)
        {
            Explode();
        }

        if (_bombVelocity == EBombVelocity.NORMAL && !_isFuzeActivate)
        {
            if (other.gameObject.TryGetComponent(out IDamagable damagableObject))
            {
                Explode();
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
        Explode();
    }

    // 폭탄 두기기
    public override void PlaceBomb(Transform fireTransform)
    {
        _fireDirection = fireTransform.right;
        _currentSpeed = 0f;
    }

    // 폭탄 던지기 (곡사)
    public override void ThrowBomb(Transform fireTransform)
    {
        _fireDirection = fireTransform.right;
        _currentSpeed = _bombStat.Speed;
        _rigidBody.AddForce(_fireDirection * _currentSpeed, ForceMode2D.Impulse);
    }

    // 폭탄 직선으로 던지기
    public override void ThrowBombStraight(Transform fireTransform)
    {
        _fireDirection = fireTransform.right;
        _currentSpeed = _bombStat.Speed * 1.4f;
        _rigidBody.AddForce(_fireDirection * _currentSpeed, ForceMode2D.Impulse);
    }

    // 폭탄 부스트
    public override void BoostBomb(Transform fireTransform)
    {
        _fireDirection = fireTransform.right;
        Explode();
    }

    // 폭탄 내려 찍기
    public override void SmashBomb(Transform fireTransform)
    {
        _fireDirection = fireTransform.right;
        _currentSpeed = _bombStat.Speed;
        _rigidBody.AddForce(_fireDirection * _currentSpeed, ForceMode2D.Impulse);
    }
}
