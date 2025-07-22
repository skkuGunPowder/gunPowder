using UnityEngine;

public class MissileExplosion : Explosion
{
    public const string ID = "E0004";


    private void Awake()
    {
        SetStat(ID);
    }
}
