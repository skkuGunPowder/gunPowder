using UnityEngine;

public class MissileBomb : Bomb
{
    public const string ID = "B0004";

    private const float PREDELAY = 0.3f;
    private float _timer;


    protected override void Init()
    {
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

        transform.position += _fireDirection * _currentSpeed * Time.deltaTime;
    }

    public override void PlaceBomb(Transform fireTransform)
    {
        ThrowBomb(fireTransform);
    }

    public override void ThrowBomb(Transform fireTransform)
    {
        _fireDirection = fireTransform.right;
        _currentSpeed = _bombStat.Speed;
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
