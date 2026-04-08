using System.Globalization;
using System.Collections.Generic;
using LitJson;

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
    public readonly CartridgeRarity Rarity;
    public readonly int Durability;
    public readonly string Explanation;
    public readonly List<float> GimmickValues;


    public CartridgeData(JsonData json)
    {
        ID = json["CartridgeID"].ToString();
        Rarity = (CartridgeRarity)System.Enum.Parse(typeof(CartridgeRarity), json["Rarity"].ToString());
        Durability = int.Parse(json["Durability"].ToString());
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
}
