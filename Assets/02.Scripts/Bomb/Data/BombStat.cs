using Firebase.Firestore;

[FirestoreData]
public class BombStat : IStat
{
    [FirestoreProperty] public int AttackPower {get; private set;}
    [FirestoreProperty] public float Priority {get; private set;}
    [FirestoreProperty] public int Cost {get; private set;}
    [FirestoreProperty] public float CoolTime {get; private set;}
    [FirestoreProperty] public float ExplosionRadius {get; private set;}
    [FirestoreProperty] public float ExplosivePower {get; private set;}
    [FirestoreProperty] public bool IsSelfDamage {get; private set;}
    [FirestoreProperty] public bool IsFallingOut {get; private set;}
}
