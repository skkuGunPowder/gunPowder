using UnityEngine;

public class LightweightCartridge : Cartridge
{
    // _data.GimmickValues[0] : 폭탄 쿨타임 감소 비율(0.0f ~ 1.0f)  
    public override void ExcuteGimmick(Player owner)
    {
        base.ExcuteGimmick(owner);
        // TODO: 폭탄 쿨타임 감소 매서드 호출
        // enum BombSlot { ZSlot, XSlot }
        // enum OperationType { Additive, Multiplicative }
        // owner.BombCooldownReduction(BombSlot.Z_Slot, _data.GimmickValues[0], OperationType.Multiplicative);
    }

    /*
        Player 쪽에 쿨타임 조정하는 메서드 추가 필요해용
        (예시)
        BombCooldownReduction(BombSlot slot, float value, OperationType type)
        {
            if(type == OperationType.Additive)
                _zBombCooldownTime += value;
            else if(type == OperationType.Multiplicative)
                _zBombCooldownTime *= (1 - value);
        }
    */
}
