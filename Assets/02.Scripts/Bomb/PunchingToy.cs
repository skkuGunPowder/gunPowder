using UnityEngine;
using Photon.Pun;
using System.Diagnostics;
using Cysharp.Threading.Tasks;

public class PunchingToy : Bomb, IBomb
{
    Animator _animator;
    ExplosionStat _explosionStat;

    protected override void Init()
    {
        base.Init();
        SetStat("BO0020");
        _explosionStat = ItemDatabase.Instance.GetStat<ExplosionStat>("EP0020");
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

    public async UniTaskVoid OnAnimationEnd()
    {
        // TODO: 애니메이션 끝난 후 처리
        // 애니메이션 대신 임시 딜레이 후 파괴 처리
        await UniTask.WaitForSeconds(0.3f);
        _isDestroying = true;
        DestroyCollector.Instance.PhotonLazyDestory(gameObject, PhotonView);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(_isDestroying)
        {
            return;
        }

        if(collision.gameObject == _ownerPhotonview.gameObject)
        {
            return;
        }
 
        Vector2 bounceDirection = (collision.transform.position - transform.position).normalized;
        if(collision.gameObject.CompareTag("Bomb"))
        {
            Rigidbody2D bombRB = collision.gameObject.GetComponent<Rigidbody2D>();
            bombRB.AddForce(bounceDirection * _explosionStat.ExplosivePower, ForceMode2D.Impulse);
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
            playerRB.AddForce(bounceDirection * _explosionStat.ExplosivePower, ForceMode2D.Impulse);
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
        OnAnimationEnd();
        // TODO
    }

    [PunRPC]
    public override void ThrowBombStraight(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        OnAnimationEnd();
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
