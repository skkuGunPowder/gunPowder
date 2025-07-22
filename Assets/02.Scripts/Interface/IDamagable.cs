using UnityEngine;

public interface IDamagable
{
    public void TakeDamage(int damage, Transform attacker, bool isFallingOut =false);
}
