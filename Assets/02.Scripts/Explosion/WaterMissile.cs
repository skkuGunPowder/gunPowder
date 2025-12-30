using Photon.Pun;
using UnityEngine;
using DG.Tweening;

public class WaterMissile : MonoBehaviour
{
    public VFX VFXPrefab;
    private PhotonView _attackerPhotonView;
    private ExplosionStat _stat;
    private CameraController _cameraController;
    private float _distance;

    public void Init(CameraController cameraController, PhotonView attackerPhotonView, float distance, ExplosionStat stat)
    {
        _cameraController = cameraController;
        _attackerPhotonView = attackerPhotonView;
        _distance = distance;
        _stat = stat;
    }

    public void Launch(Vector3 direction)
    {
        transform.DOMove(transform.position + direction * _distance, 0.3f)
        .OnComplete(() =>
        {
            Destroy(gameObject, 0.2f); 
        });
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "TileMap")
        {
            Destroy(gameObject);
        }

        if (collision.tag == "Player" || collision.tag == "Enemy")
        {
            Explode();
        }
    }

    private void Explode()
    {
        VFXPool.Instance.Play(VFXPrefab.name, transform.position);

        if (_cameraController != null)
        {
            _cameraController.ExplosionShake(transform, _stat.ExplosionRadius);
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _stat.ExplosionRadius);
        foreach (Collider2D other in colliders)
        {
            if (other.gameObject.tag == "Immune")
            {
                continue;
            }

            if (other.TryGetComponent(out IDamagable damagableObject))
            {
                damagableObject.TakeDamage(_stat.AttackPower, _stat.AttackPower, _stat.HealPercent, transform.position, _attackerPhotonView.ViewID, _attackerPhotonView.OwnerActorNr);
                if (other.TryGetComponent(out Rigidbody2D otherRigidBody))
                {
                    AddExplosionForce2D(otherRigidBody, _stat.ExplosivePower, transform.position, _stat.ExplosionRadius);
                }
            }
        }
    }

    public void AddExplosionForce2D(Rigidbody2D rb, float explosionForce, Vector2 explosionPosition, float explosionRadius)
    {
        // 슈퍼아머가 활성화된 플레이어는 힘을 받지 않음
        Player player = rb.GetComponent<Player>();
        if (player != null && player.IsSuperArmorEnabled)
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

        rb.AddForce(direction * forceMagnitude, ForceMode2D.Impulse);
    }
}
