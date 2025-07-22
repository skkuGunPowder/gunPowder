using UnityEngine;

public interface IDamagable
{
    public void TakeDamage(int damage,Transform attackerBomb, Transform attacker, bool isFallingOut =false);
}
