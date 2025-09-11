using Photon.Pun;
using UnityEngine;

public class SuicideUltiExplosion : Explosion
{
    public const string ID = "EP0019";

    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }

    public override void Explode(bool isFallingOut, PhotonView attackerPhotonView)
    {
        VFXPool.Instance.Play(VFXPrefab.name, transform.position);

        Rigidbody2D rigidbody = attackerPhotonView.GetComponent<Rigidbody2D>();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _stat.ExplosionRadius);
        var processedRBs = new System.Collections.Generic.HashSet<Rigidbody2D>();
        var processedRoots = new System.Collections.Generic.HashSet<Transform>();
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
                    // 중복 처리 방지: 같은 Rigidbody2D에 대해 한 번만 처리
                    if (!processedRBs.Add(otherRigidBody))
                    {
                        continue;
                    }
                    AddExplosionForce2D(otherRigidBody, _stat.ExplosivePower * rigidbody.linearVelocity.magnitude, transform.position, _stat.ExplosionRadius);
                }
                else
                {
                    // Rigidbody가 없는 대상은 루트 기준으로 한 번만 처리
                    Transform root = other.transform.root;
                    if (!processedRoots.Add(root))
                    {
                        continue;
                    }
                }

                if (attackerPhotonView.IsMine && !_stat.IsSelfDamage)
                {
                    continue;
                }
                int damage = DamagePerDistance(other, otherRigidBody, transform.position, _stat.ExplosionRadius, _stat.AttackPower);
                damagableObject.TakeDamage(damage, _stat.AttackPower, _stat.HealPercent, transform.position, attackerPhotonView.ViewID, attackerPhotonView.OwnerActorNr, isFallingOut);
            }
        }
        ExplosionPool.Instance.Return(gameObject.name, gameObject.GetComponent<Explosion>());
    }
}
