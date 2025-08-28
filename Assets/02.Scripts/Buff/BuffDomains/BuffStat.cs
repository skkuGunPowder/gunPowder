using LitJson;
using UnityEngine;

public class BuffStat
{
    public readonly float Duration;
    public readonly int Value;

    public BuffStat(JsonData json)
    {
        if (json == null)
        {
            throw new System.Exception("Json 데이터가 비어있습니다.");
        }

        Duration = (float)json["Duration"];
        Value = (int)json["Value"];
    }

    public BuffStat(float duration, int value)
    {
        if (duration < -1)
        {
            throw new System.Exception("Duration은 -1보다 작을 수 없습니다.");
        }

        Duration = duration;
        Value = value;
    }
}
