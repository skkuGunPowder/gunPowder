using Firebase.Firestore;

[FirestoreData]
public class BombStat : IStat
{
    [FirestoreProperty] public int Priority { get; private set; }
    [FirestoreProperty] public int Cost { get; private set; }
    [FirestoreProperty] public float CoolTime { get; private set; }
    [FirestoreProperty] public float Speed { get; private set; }
    [FirestoreProperty] public float FuzeTime { get; private set; }
    [FirestoreProperty] public bool IsFallingOut { get; private set; }
    [FirestoreProperty] public string ExplosionID { get; private set; }
}
