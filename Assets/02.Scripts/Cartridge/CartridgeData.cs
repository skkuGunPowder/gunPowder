using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public enum CartridgeRarity
{
    Common,
    Rare,
    Epic
}

public class CartridgeData
{
    public readonly string ID;
    public readonly CartridgeRarity Rarity;
    public readonly int Durability;
    public readonly string Explanation;
    public readonly List<float> GimmickValues;


    CartridgeData(JsonData json)
    {
        ID = json["ID"].ToString();
        Rarity = (CartridgeRarity)System.Enum.Parse(typeof(CartridgeRarity), json["Rarity"].ToString());
        Durability = int.Parse(json["Durability"].ToString());
        Explanation = json["Explanation"].ToString();
        GimmickValues = new List<float>(json["GimmickValues"].Count);
        for (int i = 0; i < json["GimmickValues"].Count; i++)
        {
            GimmickValues.Add(float.Parse(json["GimmickValues"][i].ToString()));
        }
    }
}
