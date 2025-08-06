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
        Priority = int.Parse(json["Priority"].ToString());
        Cost = int.Parse(json["Cost"].ToString());
        CoolTime = float.Parse(json["CoolTime"].ToString());
        Speed = float.Parse(json["ThrowingSpeed"].ToString());
        FuzeTime = float.Parse(json["AutoExplodeDelay"].ToString());
        IsFallingOut = bool.Parse(json["IsFallingOut"].ToString());
        ExplosionID = json["ExplosionID"].ToString();
    }
}
