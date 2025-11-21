using System.Collections;
using DG.Tweening;
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
        if(_isDestroying)
        {
            return;
        }

        if (_stat == null)
        {
            return;
        }

        _cameraController.SmallShakeAt(transform, 2f);

        _fuzeTimer += Time.deltaTime;
        if (_fuzeTimer >= _stat.FuzeTime)
        {
            _fuzeTimer = 0f;
            _isDestroying = true;
            SoundManager.Instance.StopLoopSound("BounceBombUlt_2");

            transform.DOKill();
            StopAllCoroutines();
            if(PhotonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(_isDestroying)
        {
            return;
        }

        if(collision.gameObject.GetComponent<PhotonView>() == _ownerPhotonview || collision.gameObject.tag == "Immune")
        {
            return;
        }

        PhotonView.RPC(nameof(Explode), RpcTarget.All);
        // if(photonView.IsMine)
        // {
        // }

        if (collision.collider.CompareTag("Wall"))
        {
            Vector2 randomNormal = (collision.contacts[0].normal + new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f))).normalized;
            Vector2 reflectDir = Vector2.Reflect(_rigidBody.linearVelocity.normalized, randomNormal);
            _rigidBody.linearVelocity = reflectDir * _stat.Speed;

            SoundManager.Instance.PlayLocalSound("BounceBombUlt_3", transform);
        }
    }

    [PunRPC]
    public override void Explode()
    {
        if(_isDestroying)
        {
            return;
        }

        Explosion endExplosion = ExplosionPool.Instance.Get(EndExplosion.name);
        endExplosion.transform.position = transform.position;
        endExplosion.transform.rotation = Quaternion.identity;
        endExplosion.Explode(_stat.IsFallingOut, _ownerPhotonview);
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
