using UnityEngine;

public class BasicBomb : Bomb
{
    public const string ID = "B0001";

    protected override void Init()
    {
        SetStat(ID);
    }

    protected override void Update()
    {
        base.Update();

        transform.position += _fireDirection * _currentSpeed * Time.deltaTime;
    }

    // 폭탄 두기기
    public override void PlaceBomb(Transform fireTransform)
    {
        _fireDirection = fireTransform.right;
        _currentSpeed = 0f;
    }

    // 폭탄 던지기 (곡사)
    public override void ThrowBomb(Transform fireTransform)
    {
        _fireDirection = fireTransform.right;
        _currentSpeed = _bombStat.Speed;
    }

    // 폭탄 직선으로 던지기
    public override void ThrowBombStraight(Transform fireTransform)
    {
        _fireDirection = fireTransform.right;
        _currentSpeed = _bombStat.Speed * 1.4f;
    }

    // 폭탄 부스트
    public override void BoostBomb(Transform fireTransform)
    {
        _fireDirection = fireTransform.right;
        Explode();
    }

    // 폭탄 내려 찍기
    public override void SmashBomb(Transform fireTransform)
    {
        _fireDirection = fireTransform.right;
        _currentSpeed = _bombStat.Speed;
    }
}
