// using System.Diagnostics;
using LitJson;
using UnityEngine;


public class BombStat : IStat
{
    public readonly int Priority;
    public readonly int Cost;
    public readonly float CoolTime;
    public readonly float Speed;
    public readonly float FuzeTime;
    public readonly bool IsFallingOut;
    public readonly string ExplosionID;

    public BombStat(JsonData json)
    {
        Priority = int.Parse(json["Priority"]["S"].ToString());
        Cost = int.Parse(json["Cost"]["S"].ToString());
        CoolTime = float.Parse(json["CoolTime"]["S"].ToString());
        Speed = float.Parse(json["ThrowingSpeed"]["S"].ToString());
        FuzeTime = float.Parse(json["AutoExplodeDelay"]["S"].ToString());
        IsFallingOut = bool.Parse(json["IsFallingOut"]["S"].ToString());
        ExplosionID = json["ExplosionID"]["S"].ToString();
    }
}
