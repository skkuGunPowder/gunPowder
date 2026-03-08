using LitJson;


public class ExplosionStat : IStat
{
    public readonly int AttackPower;
    public readonly int StealPercent;
    public readonly float ExplosionRadius;
    public readonly float ExplosivePower;
    public readonly float MaxStunTime;
    public readonly bool IsSelfDamage;

    public ExplosionStat(JsonData json)
    {
        AttackPower = int.Parse(json["AttackPower"].ToString());
        StealPercent = int.Parse(json["StealPercent"].ToString());
        ExplosionRadius = float.Parse(json["ExplosionRadius"].ToString());
        ExplosivePower = float.Parse(json["ExplosivePower"].ToString());
        MaxStunTime = float.Parse(json["MaxStunTime"].ToString());
        IsSelfDamage = bool.Parse(json["IsSelfDamage"].ToString());
    }
}
