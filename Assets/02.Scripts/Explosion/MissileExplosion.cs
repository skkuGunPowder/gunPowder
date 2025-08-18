using Photon.Pun;
using UnityEngine;

public class MissileExplosion : Explosion
{
    public const string ID = "EP0005";


    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }

    public override void Explode(bool isFallingOut, PhotonView attackerPhotonView)
    {
        _cameraController.ExplosionShake(transform, _stat.ExplosionRadius);
        base.Explode(isFallingOut, attackerPhotonView);
    }
}
