

public class SuicideExplosion : Explosion
{
    public const string ID = "EP0018";


    protected override void Awake()
    {
        base.Awake();
        SetStat(ID);
    }
}
