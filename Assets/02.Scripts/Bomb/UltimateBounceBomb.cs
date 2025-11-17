using System.Collections;
using Photon.Pun;
using UnityEngine;

public class UltimateBounceBomb : Bomb
{
    private const string ID = "BO0012";
    public Explosion EndExplosion;

    private CameraController _cameraController;

    protected override void Init()
    {
        base.Init();
        SetStat(ID);
        _cameraController = Camera.main.GetComponent<CameraController>();
    }

    protected override void Update()
    {
        base.Update();
        _cameraController.SmallShakeAt(transform, 2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Vector2 randomNormal = (collision.contacts[0].normal + new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f))).normalized;
            Vector2 reflectDir = Vector2.Reflect(_rigidBody.linearVelocity.normalized, randomNormal);
            _rigidBody.linearVelocity = reflectDir * _stat.Speed;

            SoundManager.Instance.PlayLocalSound("BounceBombUlt_3", transform);
        }

        if (collision.gameObject.GetComponent<IDamagable>() != null)
        {
            PhotonView.RPC(nameof(Explode), RpcTarget.All);
        }
    }

    [PunRPC]
    public override void Explode()
    {
        Explosion endExplosion = ExplosionPool.Instance.Get(EndExplosion.name);
        endExplosion.transform.position = transform.position;
        endExplosion.transform.rotation = Quaternion.identity;
        endExplosion.Explode(_stat.IsFallingOut, _ownerPhotonview);
        
        if (PhotonView.IsMine && _fuzeTimer >= _stat.FuzeTime)
        {
            SoundManager.Instance.StopLoopSound("BounceBombUlt_2");
            StartCoroutine(DestoryCoroutine(gameObject));
            return;
        }

    }

    private IEnumerator DestoryCoroutine(GameObject toDestroyObject)
    {
        yield return null;
        PhotonNetwork.Destroy(toDestroyObject);
    }

    [PunRPC]
    public override void ThrowBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _rigidBody.linearVelocity = _fireDirection * _stat.Speed;
        SoundManager.Instance.PlayLocalSound("BounceBombUlt_1", transform);
        SoundManager.Instance.PlayLocalSound("BounceBombUlt_2", transform, 0, true);
    }

    [PunRPC]
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void ThrowBombStraight(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void BoostBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void SmashBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }
}
