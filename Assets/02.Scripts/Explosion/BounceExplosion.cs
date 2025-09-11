
public class BounceExplosion : Explosion
{
    public const string ID = "EP0011";


    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}
