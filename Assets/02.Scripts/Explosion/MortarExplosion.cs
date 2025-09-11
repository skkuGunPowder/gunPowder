
public class MortarExplosion : Explosion
{
    public const string ID = "EP0009";


    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}
