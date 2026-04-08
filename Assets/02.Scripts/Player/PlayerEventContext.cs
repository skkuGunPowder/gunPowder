using UnityEngine;

public struct DeathContext
{
    public int KillerActorNumber;    // -1이면 자살/환경
    public bool IsLastKill;
    public bool IsNormalAttack;
}

public struct AttackHitContext
{
    public int VictimActorNumber;
    public int Damage;
    public int MaxDamage;
    public bool IsCritical;          // damage == maxDamage
    public Vector3 VictimPosition;
}

public struct KillContext
{
    public int VictimActorNumber;
    public bool IsLastKill;
    public bool IsNormalAttack;
}

public struct DamagedContext
{
    public int Damage;
    public int MaxDamage;
    public int AttackerActorNumber;
    public int AttackerViewId;
    public Vector3 AttackerBombPosition;
    public int StealPercent;
    public bool IsFallingOut;
    public bool IsNormalAttack;
}
