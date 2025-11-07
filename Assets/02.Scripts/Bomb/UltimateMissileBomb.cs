using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using System.Collections;

public class UltimateMissileBomb : Bomb
{
    public const string ID = "BO0006";
    protected const float PREDELAY = 0.3f;

    protected override void Init()
    {
        base.Init();
        SetStat(ID);
    }


    protected override void Update()
    {
        base.Update();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Owner null 체크 추가
        if (_ownerPhotonview != null && other.gameObject == _ownerPhotonview.gameObject)
        {
            return;
        }

        if (other.gameObject.tag == "Bomb")
        {
            return;
        }

        if (other.gameObject.tag == "Immune")
        {
            return;
        }

        if (CheckPriority(other))
        {
            return;
        }

        // 소유자만 폭발 RPC 호출
        if (PhotonView.IsMine && !isDestroyed)
        {
            PhotonView.RPC(nameof(Explode), RpcTarget.All);
        }
    }

    [PunRPC]
    public override void ThrowBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        transform.DORotateQuaternion(Quaternion.LookRotation(fireFowordDirection, fireUpDrection), PREDELAY).OnComplete(() =>
        {
            _vfx.gameObject.SetActive(true);
            StartCoroutine(AccelerateForward(_fireDirection, 0.5f, _stat.Speed));
        });
    }

    protected IEnumerator AccelerateForward(Vector3 direction, float accelTime, float maxSpeed)
    {
        float timer = 0f;
        while (timer < accelTime && !isDestroyed)
        {
            float t = timer / accelTime;
            _currentSpeed = Mathf.Lerp(0f, maxSpeed, t);
            // Transform 직접 수정 대신 Rigidbody2D 사용
            Vector2 newPosition = (Vector2)transform.position + (Vector2)direction * (_currentSpeed * Time.deltaTime);
            _rigidBody.MovePosition(newPosition);
            timer += Time.deltaTime;
            yield return null;
        }
        _currentSpeed = maxSpeed;
        while (!isDestroyed)
        {
            // Transform 직접 수정 대신 Rigidbody2D 사용
            Vector2 newPosition = (Vector2)transform.position + (Vector2)direction * (_currentSpeed * Time.deltaTime);
            _rigidBody.MovePosition(newPosition);
            yield return null;
        }
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

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
