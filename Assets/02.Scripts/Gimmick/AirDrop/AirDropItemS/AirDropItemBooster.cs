using UnityEngine;

public class AirDropItemBooster : AirDropItemBase, IAirDropItem
{
    private BoosterBuff boosterBuff;

    public override void Use()
    {
        base.Use();
        
        Debug.LogWarning("부스트 아이템 사용");
        boosterBuff = (BoosterBuff)BuffManager.Instance.GetBuff("BF0002", _owner.transform);
        boosterBuff.SetOwner(_owner);
        boosterBuff.StartBuff();
    }
}
