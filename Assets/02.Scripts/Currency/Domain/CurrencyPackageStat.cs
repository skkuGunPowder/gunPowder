using LitJson;
using System;

public class CurrencyPackageStat : IStat
{
    public readonly ECurrencyType CurrencyType;

    public readonly int Amount;

    public CurrencyPackageStat(JsonData json)
    {
        if (json == null || string.IsNullOrEmpty(json.ToJson()))
        {
            throw new Exception("Json이 유효하지 않습니다.");
        }

        Amount = int.Parse(json["Amount"].ToString());
    }
}
