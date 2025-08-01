using UnityEngine;

public class BasicExplosion : Explosion
{
    public const string ID = "E0001";


    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}
