using UnityEngine;

public class MissileExplosion : Explosion
{
    public const string ID = "EP0005";


    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}
