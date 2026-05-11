using Photon.Pun;
using UnityEngine;

public class RaceCarExplosion : Explosion
{
    public const string ID = "EP0022";


    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}