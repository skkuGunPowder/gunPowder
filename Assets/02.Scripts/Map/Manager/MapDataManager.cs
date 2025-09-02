using System;
using System.Collections.Generic;
using UnityEngine;

public class MapDataManager : Singleton<MapDataManager>
{
    [Header("모든 맵")]
    [SerializeField] private List<MapDataSO> _mapDataSOList;
    [Header("모든 테마")]
    [SerializeField] private List<MapThemeDataSO> _mapThemeDataSOList;
    // 맵 이름에 맞춰 맵 데이터 불러오기
    private List<MapData> _mapDataList;
    public List<MapData> MapDataList => _mapDataList;
    // 테마에 맞게 맵 리스트 불러오기
    private Dictionary<EMapTheme, MapThemeData> _mapThemeDataDict;
    public Dictionary<EMapTheme, MapThemeData> MapThemeDataDict => _mapThemeDataDict;

    public EMap CurrentMap = EMap.Map1;
    public event Action OnMapDataLoad;
    private void Awake()
    {
        MapDataSetting();
        ThemeSetting();
    }

    private void MapDataSetting()
    {
        _mapDataList = new List<MapData>();
        
        foreach (MapDataSO dataSO in _mapDataSOList)
        {
            MapData data = new MapData(dataSO.Map, dataSO.MapName, dataSO.MapSprite);
            _mapDataList.Add(data);
        }
    }
    
    private void ThemeSetting()
    {
        _mapThemeDataDict = new Dictionary<EMapTheme, MapThemeData>();
        
        foreach (MapThemeDataSO theme in _mapThemeDataSOList)
        {
            MapThemeData themeData = new MapThemeData(theme.MapTheme, theme.ThemeName, theme.MapIcon);

            foreach (MapDataSO dataSO in _mapDataSOList)
            {
                if (dataSO.MapTheme == themeData.MapTheme)
                {
                    MapData data = new MapData(dataSO.Map, dataSO.MapName, dataSO.MapSprite);
                    themeData.AddMapData(data);
                }
            }
            
            _mapThemeDataDict.Add(themeData.MapTheme, themeData);
        }
    }

    public MapData GetDataLoad(EMap map)
    {
        foreach (MapData data in _mapDataList)
        {
            if(map != data.Map)
            {
                continue;
            }
            
            return data;
        }
        
        return null;
    }

    // 모든 맵 전달
    public void AllDataLoad()
    {
        
    }

    // 테마에 있는 모든 맵 데이터 전달
    public List<MapData> ThemeDataLoad(EMapTheme theme)
    {
        foreach (var dic in _mapThemeDataDict)
        {
            if (dic.Key != theme)
            {
                continue;
            }
            
            return dic.Value.MapDataList;
        }
        
        return null;
    }
    
    
    // 전달받은 맵이 어떤 테마인가?
    public MapThemeData GetThemeData(EMap map)
    {
        foreach (var dic in _mapThemeDataDict)
        {
            if (dic.Value.MapCheck(map))
            {
                return dic.Value;
            }
        }
        
        return null;
    }
}
