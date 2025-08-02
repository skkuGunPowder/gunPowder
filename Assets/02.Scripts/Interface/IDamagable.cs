using UnityEngine;

public interface IDamagable
{
    public void TakeDamage(int damage,Vector3 attackerBomb, int attackerViewId, int attackerActorNumber, bool isFallingOut =false);
}
