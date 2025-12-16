using UnityEngine;
using Photon.Pun;
using System.Diagnostics;

public class PunchingToy : Bomb, IBomb
{
    Animator _animator;
    ExplosionStat _explosionStat;

    protected override void Init()
    {
        base.Init();
        // TODO: 뒤끝 차트 추가되면 ID작성
        // SetStat("");
        // _explosionStat = ItemDatabase.Instance.GetStat<ExplosionStat>("");
        _animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        // Update 처리 없음
    }

    public override void Explode()
    {
        // 폭발 처리 없음
    }

    public void OnAnimationEnd()
    {
        // TODO: 애니메이션 끝난 후 처리

        _isDestroying = true;
        DestroyCollector.Instance.PhotonLazyDestory(gameObject, PhotonView);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(_isDestroying)
        {
            return;
        }

        Vector2 bounceDirection = collision.contacts[0].normal;
        if(collision.gameObject.CompareTag("Bomb"))
        {
            Rigidbody2D bombRB = collision.gameObject.GetComponent<Rigidbody2D>();
            bombRB.AddForce(-bounceDirection * _explosionStat.ExplosivePower, ForceMode2D.Impulse);
            return;
        }

        IDamagable damagableObject = collision.gameObject.GetComponent<IDamagable>();
        if(damagableObject != null)
        {
            if(collision.gameObject.CompareTag("Immune"))
            {
                return;
            }

            Rigidbody2D playerRB = collision.gameObject.GetComponent<Rigidbody2D>();
            playerRB.AddForce(-bounceDirection * _explosionStat.ExplosivePower, ForceMode2D.Impulse);
            damagableObject.TakeDamage(_explosionStat.AttackPower, _explosionStat.AttackPower, _explosionStat.HealPercent, transform.position, _ownerPhotonview.ViewID, _ownerPhotonview.OwnerActorNr);
            return;
        }
    }


    [PunRPC]
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void ThrowBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        transform.parent = _ownerPhotonview.transform;
        // TODO
    }

    [PunRPC]
    public override void ThrowBombStraight(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        transform.parent = _ownerPhotonview.transform;
        // TODO
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
