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

    public override void Explode(bool isFallingOut, PhotonView attackerPhotonView, bool isNormalAttack = false)
    {
        base.Explode(isFallingOut, attackerPhotonView, isNormalAttack);
    }
}
