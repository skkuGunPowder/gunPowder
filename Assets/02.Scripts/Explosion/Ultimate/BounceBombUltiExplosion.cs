
public class BounceBombUltiExplosion : Explosion
{
    public const string ID = "EP0012";


    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}
