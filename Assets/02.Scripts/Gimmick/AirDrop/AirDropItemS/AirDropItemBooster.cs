using UnityEngine;

public class AirDropItemBooster : AirDropItemBase, IAirDropItem
{
    private BoosterBuff boosterBuff;

    public override void Use()
    {
        base.Use();
        
        Debug.LogWarning("부스트 아이템 사용");
        if(_owner == null)
        {
            Debug.LogError($"[{name}] :: 아이템 사용자가 없습니다.");
            return;
        }
        boosterBuff = (BoosterBuff)BuffManager.Instance.GetBuff("BF0002", _owner);
        boosterBuff.StartBuff();
    }
}
