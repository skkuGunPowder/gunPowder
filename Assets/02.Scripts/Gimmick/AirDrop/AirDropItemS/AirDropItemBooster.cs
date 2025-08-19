using UnityEngine;

public class AirDropItemBooster : AirDropItemBase, IAirDropItem, IBuff
{
    private float _duration = 5f;
    

    public override void Use()
    {
        Debug.LogWarning("부스트 아이템 사용");
        SetDuration();
        StartBuff();
    }

    public void SetDuration()
    {
        _owner.SetDuration(_duration);
    }

    public void StartBuff()
    {
        _owner.PlayerStat.MaxJumpCount = 9999;
    }

    public void EndBuff()
    {
        Debug.LogWarning("부스트 아이템 종료");
        _owner.PlayerStat.MaxJumpCount = 2;
    }
}
