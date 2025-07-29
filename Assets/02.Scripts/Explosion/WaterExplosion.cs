using UnityEngine;

public class WaterExplosion : Explosion
{
    public const string ID = "E0004";


    private void Awake()
    {
        SetStat(ID);
    }
}
