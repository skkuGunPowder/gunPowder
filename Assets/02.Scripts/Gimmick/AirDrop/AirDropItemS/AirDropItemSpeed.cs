using UnityEngine;

public class AirDropItemSpeed : AirDropItemBase, IAirDropItem
{
    private SpeedUpBuff speedUpBuff;

    public override void Use()
    {
        base.Use();
        Debug.LogWarning("스피드 아이템 사용");
        speedUpBuff = (SpeedUpBuff)BuffManager.Instance.GetBuff("BF0001", _owner.transform);
        speedUpBuff.SetOwner(_owner);
        speedUpBuff.StartBuff();
    }
}
