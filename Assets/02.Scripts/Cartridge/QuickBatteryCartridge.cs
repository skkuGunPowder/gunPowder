using UnityEngine;

public class QuickBatteryCartridge : Cartridge
{
    public override bool ExcuteGimmick(Player owner)
    {
        if(!base.ExcuteGimmick(owner))
        {
            return false;
        }

        owner.ForceUltimateChance();
        return true;
    }
}
