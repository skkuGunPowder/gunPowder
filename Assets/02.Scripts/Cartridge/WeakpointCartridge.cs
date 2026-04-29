using UnityEngine;

public class WeakpointCartridge : Cartridge
{
    public override bool ExcuteGimmick(Player owner)
    {
        if(!base.ExcuteGimmick(owner))
        {
            return false;
        }
        owner.SetAlwaysMaxDamage(true);
        return true;
    }
}
