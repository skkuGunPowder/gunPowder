using UnityEngine;

public class MissileExplosion : Explosion
{
    public const string ID = "E0004";


    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}
