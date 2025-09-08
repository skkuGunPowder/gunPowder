using LitJson;


public class ExplosionStat : IStat
{
    public readonly int AttackPower;
    public readonly int HealPercent;
    public readonly float ExplosionRadius;
    public readonly float ExplosivePower;
    public readonly bool IsSelfDamage;

    public ExplosionStat(JsonData json)
    {
        AttackPower = int.Parse(json["AttackPower"].ToString());
        HealPercent = int.Parse(json["HealPercent"].ToString());
        ExplosionRadius = float.Parse(json["ExplosionRadius"].ToString());
        ExplosivePower = float.Parse(json["ExplosivePower"].ToString());
        IsSelfDamage = bool.Parse(json["IsSelfDamage"].ToString());
    }
}
