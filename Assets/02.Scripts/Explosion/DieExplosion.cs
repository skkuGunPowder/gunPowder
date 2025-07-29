using Photon.Pun;
using UnityEngine;

public class DieExplosion : Explosion
{
    public const string ID = "E0002";

    private void Awake()
    {
        SetStat(ID);
    }
}
