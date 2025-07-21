using Firebase.Firestore;

[FirestoreData]
public class BombStat : IStat
{
    [FirestoreProperty] public int AttackPower {get; private set;}
    [FirestoreProperty] public int Priority {get; private set;}
    [FirestoreProperty] public int Cost {get; private set;}
    [FirestoreProperty] public float ExplosionRadius {get; private set;}
    [FirestoreProperty] public float ExplosivePower {get; private set;}
    [FirestoreProperty] public float Speed { get; private set; }
    [FirestoreProperty] public float FuzeTime { get; private set; }
    [FirestoreProperty] public bool IsSelfDamage {get; private set;}
    [FirestoreProperty] public bool IsFallingOut {get; private set;}
}
