using UnityEngine;

public interface IDamagable
{
    public void TakeDamage(int damage, int maxDamage, int StealPercent, Vector3 attackerBomb, int attackerViewId, int attackerActorNumber, bool isFallingOut =false, bool isNormalAttack = false);
}
