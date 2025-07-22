using System.Collections;
using DG.Tweening;
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

    protected override void OnCollisionEnter2D(Collision2D other)
    {
        base.OnCollisionEnter2D(other);

        if (other.gameObject.tag == "Player")
        {
            return;
        }

        Explode();
    }


    public override void PlaceBomb(Transform fireTransform)
    {
        ThrowBomb(fireTransform);
    }

    public override void ThrowBomb(Transform fireTransform)
    {
        _fireTransform = fireTransform;
        _fireDirection = _fireTransform.right;

        transform.DORotateQuaternion(Quaternion.LookRotation(_fireTransform.forward, _fireTransform.up), 0.3f)
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

    public override void ThrowBombStraight(Transform fireTransform)
    {
        ThrowBomb(fireTransform);
    }

    public override void BoostBomb(Transform fireTransform)
    {
        ThrowBomb(fireTransform);
    }

    public override void SmashBomb(Transform fireTransform)
    {
        ThrowBomb(fireTransform);
    }
}
