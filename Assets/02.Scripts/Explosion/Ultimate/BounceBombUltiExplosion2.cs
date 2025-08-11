using UnityEngine;

public class BounceBombUltiExplosion2 : Explosion
{
    public const string ID = "EP0013";


    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}
