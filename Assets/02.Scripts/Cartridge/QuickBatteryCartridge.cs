using UnityEngine;

public class QuickBatteryCartridge : Cartridge
{
    public override void ExcuteGimmick(Player owner)
    {
        base.ExcuteGimmick(owner);

        owner.ForceUltimateChance();
    }
}
