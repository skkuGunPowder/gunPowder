using Photon.Pun;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public GameObject VFXPrefab;

    protected ExplosionStat _stat;
    protected CameraController _cameraController;
    [SerializeField]
    private bool _debugDamageLog = false;

    protected virtual void Awake()
    {
        _cameraController = Camera.main.GetComponent<CameraController>();
    }

    protected void SetStat(string id)
    {
        _stat = ItemDatabase.Instance.GetStat<ExplosionStat>(id);
    }

    public virtual void Explode(bool isFallingOut, PhotonView attackerPhotonView, bool isNormalAttack = false)
    {
        
        VFXPool.Instance.Play(VFXPrefab.name, transform.position);
        
        Player attackerPlayer = attackerPhotonView.GetComponent<Player>();

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
                    AddExplosionForce2D(otherRigidBody, _stat.ExplosivePower, transform.position, _stat.ExplosionRadius);
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

                if (other.gameObject == attackerPhotonView.gameObject && !_stat.IsSelfDamage)
                {
                    continue;
                }
                
                int damage = _stat.AttackPower;
                if(attackerPlayer == null || !attackerPlayer.IsAlwaysMaxDamage)
                {
                    damage = DamagePerDistance(other, otherRigidBody, transform.position, _stat.ExplosionRadius, _stat.AttackPower);
                }
                damagableObject.TakeDamage(damage, _stat.AttackPower, _stat.StealPercent, transform.position, attackerPhotonView.ViewID, attackerPhotonView.OwnerActorNr, _stat.MaxStunTime, isFallingOut, isNormalAttack);

                // 공격 적중 이벤트 발행 (공격자 로컬에서만)
                if (attackerPhotonView.IsMine)
                {
                    Player victim = other.GetComponent<Player>();
                    int victimActorNr = victim != null ? victim.ActorNumber : -1;
                    PlayerEventManager.Instance.GetEvents(attackerPhotonView.OwnerActorNr).InvokeOnAttackHit(new AttackHitContext
                    {
                        VictimActorNumber = victimActorNr,
                        Damage = damage,
                        MaxDamage = _stat.AttackPower,
                        IsCritical = damage == _stat.AttackPower,
                        VictimPosition = other.transform.position
                    });
                    // 공격자측 적중 파티클을 로컬에서 즉시 트리거 (client-side prediction).
                    // 기존 경로는 victim의 RPC_TakeDamage가 attacker.RPC(SpawnAttackerHitParticles)를 호출해
                    // 풀 RTT만큼 지연됐음 → 이제 풀 RTT 회피.
                    // 데미지 권위 처리 자체는 그대로 RPC_TakeDamage에서 진행됨.
                    if (victim != null && victim.PhotonView != null
                        && other.gameObject != attackerPhotonView.gameObject)
                    {
                        PlayerStat attackerStat = attackerPlayer != null ? attackerPlayer.GetComponent<PlayerStat>() : null;
                        PlayerStat victimStat = victim.GetComponent<PlayerStat>();
                        bool isSameTeam = attackerStat != null && victimStat != null
                            && attackerStat.Team == victimStat.Team
                            && attackerPhotonView.OwnerActorNr != victim.ActorNumber;
                        if (!isSameTeam)
                        {
                            PlayerDamageController dmgController = attackerPhotonView.GetComponent<PlayerDamageController>();
                            if (dmgController != null)
                            {
                                bool isCrit = (damage == _stat.AttackPower);
                                dmgController.SpawnAttackerHitParticles(
                                    other.transform.position,
                                    isCrit,
                                    victim.PhotonView.ViewID);
                            }
                        }
                    }
                }
            }
        }
        ExplosionPool.Instance.Return(gameObject.name, gameObject.GetComponent<Explosion>());
    }

    public void AddExplosionForce2D(Rigidbody2D rb, float explosionForce, Vector2 explosionPosition, float explosionRadius)
    {
        // 슈퍼아머가 활성화된 플레이어는 힘을 받지 않음
        // 단, 폭탄 대시 중일 때는 예외적으로 힘을 허용
        Player player = rb.GetComponent<Player>();
        if (player != null && player.IsSuperArmorEnabled && !player.AllowBombDashForce)
        {
            return;
        }

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

        // 히트스탑 중 마지막 폭발 넉백을 위해 폭발 정보 저장
        if (player != null)
        {
            player.StoreLastExplosionInfo(explosionForce, explosionPosition, explosionRadius);
        }
    }
    
    // 거리별 데미지 계산: 폭발 중심에서 콜라이더 표면까지의 최단거리 사용
    public int DamagePerDistance(Collider2D hitCollider, Rigidbody2D rb, Vector2 explosionPosition, float explosionRadius, int maxDamage)
    {
        if (hitCollider == null)
        {
            return 0;
        }

        Transform root = rb != null ? rb.transform : hitCollider.transform;
        float distance = Mathf.Min(GetClosestDistanceToRoot(root, explosionPosition), explosionRadius);

        if (distance >= explosionRadius)
        {
            return 0;
        }

        float t = 1f - (distance / explosionRadius);
        float damagePerDistance = maxDamage * t;

        if (_debugDamageLog)
        {
            Debug.Log($"damagePerDistance: {damagePerDistance}, distance: {distance}, explosionRadius: {explosionRadius}, damage: {Mathf.CeilToInt(Mathf.Max(0f, damagePerDistance))}");
        }

        return Mathf.CeilToInt(Mathf.Max(0f, damagePerDistance));
    }

    // 루트 트랜스폼 하위의 모든 Collider2D 중 폭발 원점까지의 최단거리
    private static float GetClosestDistanceToRoot(Transform root, Vector2 origin)
    {
        float minDistance = float.PositiveInfinity;
        var colliders = root.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            var col = colliders[i];
            if (col == null || !col.enabled) continue;

            if (col.OverlapPoint(origin))
            {
                return 0f;
            }

            Vector2 closest = col.ClosestPoint(origin);
            float d = Vector2.Distance(origin, closest);
            if (d < minDistance) minDistance = d;
        }

        if (float.IsPositiveInfinity(minDistance))
        {
            return Vector2.Distance(origin, (Vector2)root.position);
        }
        return minDistance;
    }
}
