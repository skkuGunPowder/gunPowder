public class WaterExplosion : Explosion
{
    public const string ID = "E0005";

    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}
