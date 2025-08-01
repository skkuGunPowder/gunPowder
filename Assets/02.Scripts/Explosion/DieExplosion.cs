using Photon.Pun;
using UnityEngine;

public class DieExplosion : Explosion
{
    public const string ID = "E0002";

    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}
