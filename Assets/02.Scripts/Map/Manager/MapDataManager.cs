using System;
using System.Collections.Generic;
using UnityEngine;

public class MapDataManager : MonoBehaviour
{
    [SerializeField] private List<MapDataSO> _mapDataSOList;
    [SerializeField] private List<MapThemeDataSO> _mapThemeDataSOList;
    
    private Dictionary<EMapTheme, MapThemeData> _mapThemeDataDict;
    public Dictionary<EMapTheme, MapDataSO> MapThemeDataList;

    private void Awake()
    {
        foreach (MapThemeDataSO theme in _mapThemeDataSOList)
        {
            MapThemeData themeData = new MapThemeData(theme.MapTheme, theme.ThemeName, theme.MapIcon);

            foreach (var dataSO in _mapDataSOList)
            {
                if (dataSO.MapTheme == themeData.MapTheme)
                {
                    // themeData.AddMapData(dataSO);
                    continue;
                }
            }
            // theme
        }
    }

    private void DataSetting()
    {
        foreach (var dataSo in _mapDataSOList)
        {
            
        }
    }
}
