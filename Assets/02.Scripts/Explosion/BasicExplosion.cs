using Photon.Pun;
using UnityEngine;

public class BasicExplosion : Explosion
{
    public const string ID = "EP0001";


    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }

    public override void Explode(bool isFallingOut, PhotonView attackerPhotonView)
    {
        base.Explode(isFallingOut, attackerPhotonView);
    }
}
