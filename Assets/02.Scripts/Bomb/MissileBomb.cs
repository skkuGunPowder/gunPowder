using System.Collections;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;
public class MissileBomb : Bomb
{
    public const string ID = "BO0005";
    private const float PREDELAY = 0.3f;



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
        if (other.gameObject == _ownerPhotonview.gameObject)
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
    
        PhotonView.RPC(nameof(Explode), RpcTarget.All);
    }

    [PunRPC]
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void ThrowBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        transform.DORotateQuaternion(Quaternion.LookRotation(fireFowordDirection, fireUpDrection), PREDELAY)
        .OnComplete(() =>
        {
            _vfx.gameObject.SetActive(true);
            StartCoroutine(AccelerateForward(_fireDirection, 0.5f, _stat.Speed));
        });
    }

    private IEnumerator AccelerateForward(Vector3 direction, float accelTime, float maxSpeed)
    {
        float timer = 0f;
        while (timer < accelTime)
        {
            float t = timer / accelTime;
            _currentSpeed = Mathf.Lerp(0f, maxSpeed, t);
            transform.position += direction * _currentSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }
        _currentSpeed = maxSpeed;
        while (true)
        {
            transform.position += direction * _currentSpeed * Time.deltaTime;
            yield return null;
        }
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
        // 코루틴 중지
        StopAllCoroutines();
    }
}