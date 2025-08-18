using UnityEngine;

public interface IDamagable
{
    public void TakeDamage(int damage, int maxDamage,Vector3 attackerBomb, int attackerViewId, int attackerActorNumber, bool isFallingOut =false);
}
