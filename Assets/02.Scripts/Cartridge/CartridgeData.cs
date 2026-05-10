using System.Globalization;
using System.Collections.Generic;
using LitJson;
using UnityEngine.AddressableAssets;
using UnityEngine;

public enum CartridgeRarity
{
    Common,
    Rare,
    Epic,
    Ultimate
}

public class CartridgeData
{
    public readonly string ID;
    public readonly string ImageAddress;
    public readonly Sprite ImageSprite;
    public readonly string Name;
    public readonly CartridgeRarity Rarity;
    public readonly int Durability;
    public readonly string Explanation;
    public readonly List<float> GimmickValues;


    public CartridgeData(JsonData json)
    {
        ID = json["CartridgeID"].ToString();
        ImageAddress = json["ImageAddress"].ToString();
        ImageSprite = Addressables.LoadAssetAsync<Sprite>(ImageAddress).WaitForCompletion(); // TODO: 아직 어드레서블 이미지 없음
        Name = json["Name"].ToString();
        Rarity = (CartridgeRarity)System.Enum.Parse(typeof(CartridgeRarity), json["Rarity"].ToString());
        Durability = int.Parse(json["Durability"].ToString());
        Debug.LogWarning($"ID : {ID} DURABILITY : {Durability}");
        Explanation = json["Explanation"].ToString();
        GimmickValues = ParseValue(json);
    }

    private static List<float> ParseValue(JsonData json)
    {
        List<float> values = new List<float>();

        if (!json.Keys.Contains("Value"))
        {
            return values;
        }

        JsonData valueData = json["Value"];
        if (valueData == null)
        {
            return values;
        }

        if (valueData.IsArray)
        {
            for (int i = 0; i < valueData.Count; i++)
            {
                if (TryParseFloat(valueData[i].ToString(), out float parsed))
                {
                    values.Add(parsed);
                }
            }

            return values;
        }

        string rawValue = valueData.ToString();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return values;
        }

        string[] tokens = rawValue.Split(',');
        for (int i = 0; i < tokens.Length; i++)
        {
            if (TryParseFloat(tokens[i].Trim(), out float parsed))
            {
                values.Add(parsed);
            }
        }

        return values;
    }   

    private static bool TryParseFloat(string input, out float parsed)
    {
        return float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed)
            || float.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture, out parsed);
    }

    public int GetPrice()
    {
        switch (Rarity)
        {
            case CartridgeRarity.Common :
                return 20;
            case CartridgeRarity.Rare:
                return 30;
            case CartridgeRarity.Epic:
                return 45;
        }

        return 0;
    }

    // repairCount: 현재까지 수리한 횟수 (수리 전 기준)
    public int GetRepairCost(int repairCount)
    {
        int baseCost;
        int increment;
        switch (Rarity)
        {
            case CartridgeRarity.Common:
                baseCost = 10;
                increment = 5;
                break;
            case CartridgeRarity.Rare:
                baseCost = 15;
                increment = 10;
                break;
            case CartridgeRarity.Epic:
                baseCost = 20;
                increment = 15;
                break;
            default:
                return 0;
        }
        return baseCost + repairCount * increment;
    }
}
