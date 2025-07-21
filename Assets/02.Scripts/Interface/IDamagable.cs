using UnityEngine;

public interface IDamagable
{
    public void TakeDamage(int damage, Vector3 attacker, bool isFallingOut =false);
}
