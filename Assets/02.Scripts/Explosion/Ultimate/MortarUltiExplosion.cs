using UnityEngine;

public class MortarUltiExplosion : Explosion
{
    public const string ID = "EP0010";

    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}
