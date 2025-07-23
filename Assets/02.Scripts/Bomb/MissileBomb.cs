using System.Collections;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;
public class MissileBomb : Bomb
{
    public const string ID = "B0004";
    private const float PREDELAY = 0.3f;
    private float _timer;

    
    protected override void Init()
    {
        base.Init();
        SetStat(ID);
        _timer = 0f;
    }
    
    protected override void Update()
    {
        base.Update();
        _timer += Time.deltaTime;
        if (_timer < PREDELAY)
        {
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        CheckPriority(other);

        if (other.gameObject.tag == "Player")
        {
            return;
        }
        Explode();
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
        transform.DORotateQuaternion(Quaternion.LookRotation(fireFowordDirection, fireUpDrection), 0.3f)
        .OnComplete(()=>
        {
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
}