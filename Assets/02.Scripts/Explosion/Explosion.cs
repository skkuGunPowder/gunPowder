using Photon.Pun;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public GameObject VFXPrefab;

    protected ExplosionStat _stat;
    protected CameraController _cameraController;

    protected virtual void Awake()
    {
        _cameraController = Camera.main.GetComponent<CameraController>();
    }

    protected void SetStat(string id)
    {
        _stat = ItemDatabase.Instance.GetStat<ExplosionStat>(id);
    }

    public virtual void Explode(bool isFallingOut, PhotonView attackerPhotonView)
    {
        VFXPool.Instance.Play(VFXPrefab.name, transform.position);

        _cameraController.ExplosionShake(transform, _stat.ExplosionRadius);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _stat.ExplosionRadius);
        foreach (Collider2D other in colliders)
        {
            if (other.gameObject.tag == "Immune")
            {
                continue;
            }

            if (other.TryGetComponent(out IDamagable damagableObject))
            {
                if (other.TryGetComponent(out Rigidbody2D otherRigidBody))
                {
                    AddExplosionForce2D(otherRigidBody, _stat.ExplosivePower, transform.position, _stat.ExplosionRadius);
                }

                if (attackerPhotonView.IsMine && !_stat.IsSelfDamage)
                {
                    continue;
                }
                int damage = DamagePerDistance(otherRigidBody, transform.position, _stat.ExplosionRadius, _stat.AttackPower);
                damagableObject.TakeDamage(damage, transform.position, attackerPhotonView.ViewID, attackerPhotonView.OwnerActorNr, isFallingOut);
            }
        }
        ExplosionPool.Instance.Return(gameObject.name, gameObject.GetComponent<Explosion>());
    }

    public void AddExplosionForce2D(Rigidbody2D rb, float explosionForce, Vector2 explosionPosition, float explosionRadius)
    {
        Vector2 direction = rb.position - explosionPosition;
        float distance = direction.magnitude;

        // 폭발 반경 안에 있는 경우에만 적용
        if (distance > explosionRadius)
        {
            return;
        }

        // 거리 비례로 감소하는 힘
        float forceMagnitude = explosionForce * (1 - (distance / explosionRadius));
        direction.Normalize();

        // 허정범 테스트
        direction.y += 0.3f;

        rb.AddForce(direction * forceMagnitude, ForceMode2D.Impulse);
    }
    
    /// <summary>
    /// 거리별 데미지 계산
    /// </summary>
    /// <param name="rb"></param>
    /// <param name="explosionPosition"></param>
    /// <param name="explosionRadius"></param>
    /// <param name="damage"></param>
    /// <returns></returns>
    private int DamagePerDistance(Rigidbody2D rb, Vector2 explosionPosition, float explosionRadius, int damage)
    {
        Vector2 direction = rb.position - explosionPosition;
        float distance = direction.magnitude;

        if (distance > explosionRadius)
        {
            return 0;
        }

        float damagePerDistance = damage * (1 - (distance / explosionRadius));
        Debug.Log($"damagePerDistance: {Mathf.CeilToInt(damagePerDistance)}");
        return Mathf.CeilToInt(damagePerDistance);
    }
}
