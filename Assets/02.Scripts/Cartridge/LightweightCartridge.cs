

public enum BombSlot { ZSlot, XSlot }
public enum OperationType { Additive, Multiplicative }
public class LightweightCartridge : Cartridge
{
    // _data.GimmickValues[0] : 폭탄 쿨타임 감소 비율(0.0f ~ 1.0f)
    public override void ExcuteGimmick(Player owner)
    {
        base.ExcuteGimmick(owner);
        
        owner.BombCooldownReduction(BombSlot.ZSlot,_data.GimmickValues[0], OperationType.Multiplicative);
    }
}
