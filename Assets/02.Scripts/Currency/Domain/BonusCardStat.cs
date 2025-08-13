using System;
using LitJson;
using UnityEngine;

public class BonusCardStat : IStat
{
    public readonly ECurrencyType CurrencyType;
    public readonly bool IsEquipOnPurchase;
    public readonly int BonusPercent;
    public readonly int PlayCount;
    public readonly int ExpireDays;

    public BonusCardStat(JsonData json)
    {
        if (json == null || string.IsNullOrEmpty(json.ToJson()))
        {
            throw new Exception("Json이 유효하지 않습니다.");
        }

        CurrencyType = (ECurrencyType)Enum.Parse(typeof(ECurrencyType), json["CurrencyType"].ToString());
        IsEquipOnPurchase = bool.Parse(json["IsEquipOnPurchase"].ToString());
        BonusPercent = int.Parse(json["BonusPercent"].ToString());
        PlayCount = int.Parse(json["PlayCount"].ToString());
        ExpireDays = int.Parse(json["ExpireDays"].ToString());
    }
}
