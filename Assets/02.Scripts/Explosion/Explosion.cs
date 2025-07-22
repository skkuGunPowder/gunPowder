using UnityEngine;

public class Explosion : MonoBehaviour
{
    public GameObject VFXPrefab;

    protected ExplosionStat _stat;

    protected void SetStat(string id)
    {
        _stat = ItemDatabase.Instance.GetStat<ExplosionStat>(id);
    }

    public virtual void Explode(bool isFallingOut)
    {
        Instantiate(VFXPrefab, transform.position, Quaternion.identity);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _stat.ExplosionRadius);
        foreach (Collider2D other in colliders)
        {
            if (other.TryGetComponent(out IDamagable damagableObject))
            {
                damagableObject.TakeDamage(_stat.AttackPower, transform.position, isFallingOut);
            }
        }
    }
}
