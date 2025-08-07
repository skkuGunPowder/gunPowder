using Photon.Pun;
using UnityEngine;

public class CrossExplosion : Explosion
{
    public const string ID = "EP0007";

    public WaterMissile WaterMissilePrefab;

    [SerializeField] private float _distance = 10f;

    private Vector3[] _directions = { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, -1, 0) };

    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }

    public override void Explode(bool isFallingOut, PhotonView attackerPhotonView)
    {
        for (int i = 0; i < 4; i++)
        {
            WaterMissile waterMissile = Instantiate(WaterMissilePrefab, transform.position, Quaternion.identity);
            waterMissile.Init(_cameraController, attackerPhotonView, _distance, _stat);
            waterMissile.Launch(_directions[i]);
        }

        base.Explode(isFallingOut, attackerPhotonView);
    }
}
