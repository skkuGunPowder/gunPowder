using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


public class ColorPalette
{
    public static Dictionary<EColorType, Color32> ColorDictionary;

    public static void Init()
    {
        ColorDictionary = new Dictionary<EColorType, Color32>();

        var locations = Addressables.LoadResourceLocationsAsync("Colors").WaitForCompletion();
        var entries = Addressables.LoadAssetsAsync<ColorDataSO>(locations, null).WaitForCompletion();

        foreach (var entry in entries)
        {
            ColorDictionary.Add(entry.ColorType, entry.Color);
        }
    }

    public static Color32 GetTeamColor(EInGameTeam team)
    {
        EColorType color = (EColorType)(int)team;
        return ColorDictionary[color];
    }

    public static EColorType GetColorTypeByName(string name)
    {
        if (System.Enum.TryParse(name, out EColorType result))
        {
            return result;
        }

        return EColorType.Common;
    }
    
    public static Color32[] GetGradationColors(EColorType gradientType, int count)
    {
        int startIndex = (int)gradientType;
        
        Color32[] gradationColors = new Color32[count];
        
        for (int i = 0; i < count; i++)
        {
            if (!ColorDictionary.ContainsKey((EColorType)startIndex))
            {
                break;
            }

            gradationColors[i] = ColorDictionary[(EColorType)startIndex];
            startIndex++;
        }

        return gradationColors;
    }
}
