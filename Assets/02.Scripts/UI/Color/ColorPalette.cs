using System;
using System.Collections.Generic;
using UnityEngine;

public class ColorPalette : MonoBehaviour
{
    public List<ColorDataSO> ColorPaletteList;

    public Dictionary<EColorType, Color32> ColorDictionary;
    private void Awake()
    {
        ColorDictionary = new Dictionary<EColorType, Color32>();
        foreach (var color in ColorPaletteList)
        {
            ColorDictionary.Add(color.ColorType, color.Color);
        }
    }
}
