using UnityEngine;

public class AirDropItemSpeed : AirDropItemBase, IAirDropItem
{
    private SpeedUpBuff speedUpBuff;

    public override void Use()
    {
        base.Use();
        
        if (_owner == null)
        {
            Debug.LogError($"[{name}] :: 아이템 사용자가 없습니다.");
            return;
        }
        
        speedUpBuff = (SpeedUpBuff)BuffManager.Instance.GetBuff("BF0001", _owner);
        _owner.PlayerBuffHandler.AddBuff(speedUpBuff);
    }
}
