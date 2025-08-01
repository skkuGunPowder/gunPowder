using UnityEngine;

public class MissileExplosion : Explosion
{
    public const string ID = "EP0004";


    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}
