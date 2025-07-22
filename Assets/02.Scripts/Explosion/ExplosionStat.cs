using Firebase.Firestore;


[FirestoreData]
public class ExplosionStat : IStat
{
    [FirestoreProperty] public int AttackPower {get; private set;}
    [FirestoreProperty] public float ExplosionRadius {get; private set;}
    [FirestoreProperty] public float ExplosivePower {get; private set;}
    [FirestoreProperty] public bool IsSelfDamage {get; private set;}
}
