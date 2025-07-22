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

    [PunRPC]
    // 폭탄 두기기
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = 0f;
    }

    [PunRPC]
    // 폭탄 던지기 (곡사)
    public override void ThrowBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = _stat.Speed;
        _rigidBody.AddForce(_fireDirection * _currentSpeed, ForceMode2D.Impulse);
    }

    [PunRPC]
    // 폭탄 직선으로 던지기
    public override void ThrowBombStraight(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = _stat.Speed * 2f;
        _rigidBody.AddForce(_fireDirection * _currentSpeed, ForceMode2D.Impulse);
    }

    [PunRPC]
    // 폭탄 부스트
    public override void BoostBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        Explode();
    }

    [PunRPC]
    // 폭탄 내려 찍기
    public override void SmashBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = _stat.Speed;
        _rigidBody.AddForce(_fireDirection * _currentSpeed, ForceMode2D.Impulse);
    }
}