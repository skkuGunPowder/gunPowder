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
}
