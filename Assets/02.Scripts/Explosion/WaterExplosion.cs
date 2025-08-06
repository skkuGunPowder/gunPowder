public class WaterExplosion : Explosion
{
    public const string ID = "EP0007";

    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}
