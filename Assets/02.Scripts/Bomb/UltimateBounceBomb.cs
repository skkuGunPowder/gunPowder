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
            // 🔴 CRITICAL FIX: 소유자만 폭발 RPC 호출
            if (PhotonView.IsMine && !isDestroyed)
            {
                PhotonView.RPC(nameof(Explode), RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public override void Explode()
    {
        // 중복 파괴 방지
        if (isDestroyed)
        {
            return;
        }
        isDestroyed = true;

        // 타이머 종료 시 최종 폭발
        double elapsedTime = PhotonNetwork.Time - _spawnTime;
        if (elapsedTime >= _stat.FuzeTime)
        {
            Explosion endExplosion = ExplosionPool.Instance.Get(EndExplosion.name);
            endExplosion.transform.position = transform.position;
            endExplosion.transform.rotation = Quaternion.identity;

            if (_ownerPhotonview != null)
            {
                endExplosion.Explode(_stat.IsFallingOut, _ownerPhotonview);
            }
            else
            {
                Debug.LogWarning($"[UltimateBounceBomb] Owner PhotonView is null");
                endExplosion.Explode(_stat.IsFallingOut, null);
            }

            SoundManager.Instance.StopLoopSound("BounceBombUlt_2");

            // 소유자만 파괴 요청
            if (PhotonView.IsMine)
            {
                if (PhotonView != null && PhotonView.ViewID != 0)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
                else
                {
                    Debug.LogWarning($"[UltimateBounceBomb] PhotonView is invalid, destroying locally: {gameObject.name}");
                    Destroy(gameObject);
                }
            }
            return;
        }

        // 충돌 시 일반 폭발 (파괴 안 함)
        SoundManager.Instance.StopLoopSound("BounceBombUlt_2");

        Explosion explosion = ExplosionPool.Instance.Get(ExplosionPrefab.name);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;

        if (_ownerPhotonview != null)
        {
            explosion.Explode(_stat.IsFallingOut, _ownerPhotonview);
        }
        else
        {
            Debug.LogWarning($"[UltimateBounceBomb] Owner PhotonView is null for normal explosion");
            explosion.Explode(_stat.IsFallingOut, null);
        }

        // 충돌 시에도 파괴해야 함 (기존 로직 수정)
        if (PhotonView.IsMine)
        {
            if (PhotonView != null && PhotonView.ViewID != 0)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"[UltimateBounceBomb] PhotonView is invalid, destroying locally: {gameObject.name}");
                Destroy(gameObject);
            }
        }
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
