using Photon.Pun;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public GameObject VFXPrefab;

    protected ExplosionStat _stat;

    protected void SetStat(string id)
    {
        _stat = ItemDatabase.Instance.GetStat<ExplosionStat>(id);
    }

    public virtual void Explode(bool isFallingOut, Transform attacker)
    {
        Instantiate(VFXPrefab, transform.position, Quaternion.identity);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _stat.ExplosionRadius);
        foreach (Collider2D other in colliders)
        {
            // if (other.gameObject.tag == "Player" && !_stat.IsSelfDamage)
            // {
            //     continue;
            // }

            if (other.TryGetComponent(out IDamagable damagableObject))
            {
                damagableObject.TakeDamage(_stat.AttackPower, transform.position,  attacker.GetComponent<PhotonView>().ViewID, isFallingOut);
                if (other.TryGetComponent(out Rigidbody2D otherRigidBody))
                {
                    AddExplosionForce2D(otherRigidBody, _stat.ExplosivePower, transform.position, _stat.ExplosionRadius);
                }
            }
        }
    }

    void AddExplosionForce2D(Rigidbody2D rb, float explosionForce, Vector2 explosionPosition, float explosionRadius)
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

        rb.AddForce(direction * forceMagnitude, ForceMode2D.Impulse);
    }
}
