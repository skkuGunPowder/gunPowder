using UnityEngine;

public class AirDropItemSpeed : AirDropItemBase, IAirDropItem, IBuff
{
    private float _duration = 10f;
    private float _increaseAmount = 4f;

    private float _originalMoveSpeed;
    private float _originalRunSpeed;
    

    public override void Use()
    {
        Debug.LogWarning("스피드 아이템 사용");
        SetDuration();
        StartBuff();
    }

    public void SetDuration()
    {
        _owner.SetDuration(_duration);
    }

    public void StartBuff()
    {
        _originalMoveSpeed = _owner.PlayerStat.MoveSpeed;
        _originalRunSpeed = _owner.PlayerStat.RunSpeed;

        _owner.PlayerStat.MoveSpeed += _increaseAmount;
        _owner.PlayerStat.RunSpeed += _increaseAmount;
    }

    public void EndBuff()
    {
        Debug.LogWarning("스피드 아이템 버프 종료");
        _owner.PlayerStat.MoveSpeed = _originalMoveSpeed;
        _owner.PlayerStat.RunSpeed = _originalRunSpeed;
    }
}
