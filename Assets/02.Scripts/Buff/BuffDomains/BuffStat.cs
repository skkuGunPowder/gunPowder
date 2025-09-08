using System.Collections.Generic;
using LitJson;
using System;

public class BuffStat
{
    public readonly float Duration;
    public readonly List<int> ValueList;

    public BuffStat(JsonData json)
    {
        if (json == null)
        {
            throw new Exception("Json 데이터가 비어있습니다.");
        }

        Duration = float.Parse(json["Duration"].ToString());

        ValueList = new List<int>();
        string[] values = json["Value"].ToString().Split(',');
        foreach (var val in values)
        {
            if (int.TryParse(val.Trim(), out int intValue))
            {
                ValueList.Add(intValue);
            }
            else
            {
                throw new Exception($"Value '{val}'를 정수로 변환할 수 없습니다.");
            }
        }
    }
}
