using UnityEngine;

public class BasicExplosion : Explosion
{
    public const string ID = "E0001";


    private void Awake()
    {
        SetStat(ID);
    }
}
