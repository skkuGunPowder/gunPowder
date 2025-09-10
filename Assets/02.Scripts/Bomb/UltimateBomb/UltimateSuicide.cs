using System.Diagnostics;
using Photon.Pun;
using UnityEngine;

public class UltimateSuicide : Bomb
{
    private const string ID = "BO0019";

    protected override void Init()
    {
        base.Init();
        SetStat(ID);
    }

    protected override void Update()
    {
        transform.position = _ownerPhotonview.transform.position;
    }
    

    [PunRPC]
    public override void Explode()
    {
        Explosion explosion = ExplosionPool.Instance.Get(ExplosionPrefab.name);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;
        explosion.Explode(_stat.IsFallingOut, _ownerPhotonview);

        if (_vfx != null)
        {
            _vfx.transform.SetParent(transform);
        }
    }

    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {

    }
}
