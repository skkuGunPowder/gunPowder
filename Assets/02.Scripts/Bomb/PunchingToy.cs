using UnityEngine;
using Photon.Pun;
using Cysharp.Threading.Tasks;

public class PunchingToy : Bomb, IBomb
{
    ExplosionStat _explosionStat;
    Collider2D _collider;

    protected override void Init()
    {
        base.Init();
        SetStat("BO0020");
        _explosionStat = ItemDatabase.Instance.GetStat<ExplosionStat>("EP0020");
        _collider = GetComponent<Collider2D>();
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
        _isDestroying = true;
        DestroyCollector.Instance.PhotonLazyDestory(gameObject, PhotonView);
    }

    public void OnColliderOn()
    {
        _collider.enabled = true;
    }

    public void OnColliderOff()
    {
        _collider.enabled = false;
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

            // 슈퍼아머가 활성화된 플레이어는 힘을 받지 않음
            Player player = collision.gameObject.GetComponent<Player>();
            Rigidbody2D playerRB = collision.gameObject.GetComponent<Rigidbody2D>();
            if (player != null && player.IsSuperArmorEnabled)
            {
                // 힘은 적용하지 않고 데미지만 적용
                damagableObject.TakeDamage(_explosionStat.AttackPower, _explosionStat.AttackPower, _explosionStat.HealPercent, transform.position, _ownerPhotonview.ViewID, _ownerPhotonview.OwnerActorNr);
                return;
            }

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
        transform.rotation = Quaternion.LookRotation(fireFowordDirection, fireUpDrection);
        transform.parent = _ownerPhotonview.transform;
    }

    [PunRPC]
    public override void ThrowBombStraight(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        transform.rotation = Quaternion.LookRotation(fireFowordDirection, fireUpDrection);
        transform.parent = _ownerPhotonview.transform;
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
