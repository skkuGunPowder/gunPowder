using UnityEngine;

public class WeakpointCartridge : Cartridge
{
    public override void ExcuteGimmick(Player owner)
    {
        base.ExcuteGimmick(owner);
        owner.SetAlwaysMaxDamage(true);
    }
}
